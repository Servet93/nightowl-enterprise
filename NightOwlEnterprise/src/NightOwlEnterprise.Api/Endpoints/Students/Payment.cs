using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public static class Payment
{
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();
    
    public static void MapPayment<TUser>(this IEndpointRouteBuilder endpoints, string stripeCredentialSigningSecret)
        where TUser : class, new()
    {
        
        // NOTE: We cannot inject UserManager<TUser> directly because the TUser generic parameter is currently unsupported by RDG.
        // https://github.com/dotnet/aspnetcore/issues/47338
        endpoints.MapPost("/payment", async Task<Results<Ok<ConfirmIntentResult>, ValidationProblem>>
            ([FromBody] StudentRegisterRequestWithPaymentMethodId registration, HttpContext context, [FromServices] IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var errorDescriber = sp.GetRequiredService<TurkishIdentityErrorDescriber>();

            if (!userManager.SupportsUserEmail)
            {
                throw new NotSupportedException($"{nameof(MapPayment)} requires a user store with email support.");
            }
            
            var name = registration.NameSurname;
            var email = registration.Email;
            var address = registration.Address;
            var city = registration.City;
            
            if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
            {
                return IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)).CreateValidationProblem();
            }
            
            if (string.IsNullOrEmpty(name) ||
                name.Length < 3 ||
                name.ToLower().Contains("su") // :)
                )
            {
                return IdentityResult.Failed(errorDescriber.InvalidName(name)).CreateValidationProblem();
            }

            if (string.IsNullOrEmpty(address))
            {
                return IdentityResult.Failed(errorDescriber.RequiredAddress()).CreateValidationProblem();
            }

            if (!cities.Contains(city))
            {
                return IdentityResult.Failed(errorDescriber.InvalidCity(city)).CreateValidationProblem();
            }

            var userName = email.Split('@')[0];
            
            var user = new ApplicationUser()
            {
                Name = name,
                UserName = userName,
                Email = email,
                Address = address,
                City = city,
                AccountStatus  = AccountStatus.PaymentAwaiting,
                UserType = UserType.Student,
            };
            
            var result = await userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                return result.CreateValidationProblem();
            }

            var confirmPaymentResult = ConfirmPayment(user.Id, registration.PaymentMethodId, context);

            if (!string.IsNullOrEmpty(confirmPaymentResult.error))
            {
                await userManager.DeleteAsync(user);

                return TypedResults.ValidationProblem(new Dictionary<string, string[]>()
                {
                    { "PaymentIntentCreationError", new string[] {confirmPaymentResult.error}},
                });
            }
            
            return TypedResults.Ok(confirmPaymentResult);
        });

        // const string stripeCredentialSigningSecret = "whsec_7YBhw4M9aSypQq7K6fVCJnhcbduPb6yN";

        endpoints.MapPost("/payment-webhook", async (HttpContext httpContext, [FromServices] IServiceProvider sp) =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            var logger = loggerFactory.CreateLogger("Payment-Webhook");
            
            var userId = string.Empty;
            var paymentIntentId = string.Empty;
            
            try {
                
                var json = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
        
                var stripeEvent = EventUtility.ConstructEvent(json,
                    httpContext.Request.Headers["Stripe-Signature"], stripeCredentialSigningSecret);

                if (stripeEvent?.Data?.Object is null)
                {
                    logger.LogWarning("StripeEvent object is empty");
                    return Results.Empty;
                }
                
                var intent = (PaymentIntent)stripeEvent.Data.Object;

                paymentIntentId = intent.Id;
                
                var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                
                var isExist = intent.Metadata.TryGetValue("UserId", out userId);

                ApplicationUser user = null;
                
                if (isExist)
                {
                    user = await userManager.FindByIdAsync(userId);

                    if (user is null)
                    {
                        logger.LogWarning("User not found. UserId: {UserId}", userId);
                        return TypedResults.Empty;
                    }
                    
                }
                
                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        logger.LogInformation("Payment Succeeded. PaymentIntentId: {PaymentIntentId}, UserId: {UserId}", intent.Id, userId);
                        
                        var generatedPassword = PasswordGenerator.GeneratePassword(8);
                        
                        var addPasswordAsyncResult = await userManager.AddPasswordAsync(user!, generatedPassword);

                        if (!addPasswordAsyncResult.Succeeded)
                        {
                            var descriptions = addPasswordAsyncResult.Errors.Select(x => x.Description);
                            var errMsg = string.Join(",", descriptions);
                            logger.LogWarning(
                                "User password couldn't added. PaymentIntentId: {PaymentIntentId}, UserId: {UserId}, Message: {Message}",
                                paymentIntentId, userId, errMsg);
                            
                            return TypedResults.Empty;
                        }
                        
                        var studentEmailSender = sp.GetRequiredService<StudentEmailSender>();
                        
                        user.AccountStatus = AccountStatus.Active;
                        
                        var updateAsyncResult = await userManager.UpdateAsync(user);

                        if (!updateAsyncResult.Succeeded)
                        {
                            var descriptions = updateAsyncResult.Errors.Select(x => x.Description);
                            var errMsg = string.Join(",", descriptions);
                            logger.LogWarning("User couldn't updated. PaymentIntentId: {PaymentIntentId}, UserId: {UserId}, Message: {Message}", paymentIntentId, userId,
                                errMsg);

                            return TypedResults.Empty;
                        }

                        await studentEmailSender.SendSignInInfo(user!, generatedPassword);

                        break;
                    case "payment_intent.payment_failed":
                        logger.LogInformation("Payment Failure. PaymentIntentId: {PaymentIntentId}, UserId: {UserId}", intent.Id, userId);

                        // Notify the customer that payment failed

                        var deleteAsyncResult = await userManager.DeleteAsync(user!);

                        if (!deleteAsyncResult.Succeeded)
                        {
                            var descriptions = deleteAsyncResult.Errors.Select(x => x.Description);
                            var errMsg = string.Join(",", descriptions);
                            logger.LogWarning(
                                "User couldn't deleted. PaymentIntentId: {PaymentIntentId}, UserId: {UserId}, Message: {Message}",
                                paymentIntentId, userId, errMsg);
                        }
                        
                        break;
                    default:
                        // Handle other event types

                        break;
                }
                return TypedResults.Empty;

            }
            catch (StripeException e) {
                logger.LogError(e,"Payment Webhook has an error. UserId: {UserId}, PaymentIntentId: {PaymentIntentId}", userId, paymentIntentId);
            }

            return TypedResults.Empty;
        });
        
    }
    
    private static ConfirmIntentResult ConfirmPayment(Guid userId, string paymentMethodId, HttpContext httpContext)
    {
        var paymentIntentService = new Stripe.PaymentIntentService();
        PaymentIntent paymentIntent = null;
        var errorMessage = string.Empty;
        try
        {
            paymentIntent = paymentIntentService.Create(new Stripe.PaymentIntentCreateOptions
            {
                Confirm = true,
                Amount = 3000,
                Currency = "try",
                // In the latest version of the API, specifying the `automatic_payment_methods` parameter is optional because Stripe enables its functionality by default.
                AutomaticPaymentMethods = new Stripe.PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                PaymentMethod = paymentMethodId,
                // ReturnUrl = "http://localhost:5254/paymentResult.html",
                ReturnUrl = "http://localhost:5254/paymentResult",
                UseStripeSdk = true,
                MandateData = new PaymentIntentMandateDataOptions()
                {
                    CustomerAcceptance = new PaymentIntentMandateDataCustomerAcceptanceOptions()
                    {
                        Type = "online",
                        Online = new PaymentIntentMandateDataCustomerAcceptanceOnlineOptions()
                        {
                            IpAddress = httpContext.Connection.RemoteIpAddress!.ToString(),
                            UserAgent = httpContext.Request.Headers["User-Agent"].ToString(),
                        }
                    }
                },
                Metadata = new Dictionary<string, string>()
                {
                    {"UserId", userId.ToString()}
                }
            });
        }
        catch (Exception e)
        {
            errorMessage = e.Message;
        }

        return new ConfirmIntentResult(paymentIntent?.ClientSecret, paymentIntent?.Status, errorMessage);   
    }
        
    private static List<string> cities = new List<string>()
    {
        "Adana", "Adıyaman", "Afyonkarahisar", "Ağrı", "Amasya",
        "Ankara", "Antalya", "Artvin", "Aydın", "Balıkesir",
        "Bilecik", "Bingöl", "Bitlis", "Bolu", "Burdur",
        "Bursa", "Çanakkale", "Çankırı", "Çorum", "Denizli",
        "Diyarbakır", "Edirne", "Elazığ", "Erzincan", "Erzurum",
        "Eskişehir", "Gaziantep", "Giresun", "Gümüşhane", "Hakkâri",
        "Hatay", "Isparta", "İçel (Mersin)", "İstanbul", "İzmir",
        "Kars", "Kastamonu", "Kayseri", "Kırklareli", "Kırşehir",
        "Kocaeli", "Konya", "Kütahya", "Malatya", "Manisa",
        "Kahramanmaraş", "Mardin", "Muğla", "Muş", "Nevşehir",
        "Niğde", "Ordu", "Rize", "Sakarya", "Samsun",
        "Siirt", "Sinop", "Sivas", "Tekirdağ", "Tokat",
        "Trabzon", "Tunceli", "Şanlıurfa", "Uşak", "Van",
        "Yozgat", "Zonguldak", "Aksaray", "Bayburt", "Karaman",
        "Kırıkkale", "Batman", "Şırnak", "Bartın", "Ardahan",
        "Iğdır", "Yalova", "Karabük", "Kilis", "Osmaniye", "Düzce"
    };

    public record ConfirmIntentResult(string clientSecret, string status, string error);
    
    private sealed class StudentRegisterRequestWithPaymentMethodId
    {
        [DefaultValue("NightOwl")]
        public required string NameSurname { get; init; }
        
        [DefaultValue("nightowl-enterprise@gmail.com")]
        public required string Email { get; init; }
        
        public required string Address { get; init; }
        
        public required string City { get; init; }
        
        public required string PaymentMethodId { get; init; }
        
    } 
    
    public class PasswordGenerator
    {
        private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string NumericChars = "0123456789";
        // private const string SpecialChars = "!@#$%^&*()-_=+";

        public static string GeneratePassword(int length = 8)
        {
            // Rastgele sayı üreticisi
            using (var rng = new RNGCryptoServiceProvider())
            {
                // Şifre karakterleri
                var chars = LowercaseChars + UppercaseChars + NumericChars;

                // Şifre karakterlerini karıştır
                var shuffledChars = new string(chars.OrderBy(x => Guid.NewGuid()).ToArray());

                // Rastgele şifre oluştur
                var password = new char[length];
                var randomBytes = new byte[length];
                rng.GetBytes(randomBytes);
                for (int i = 0; i < length; i++)
                {
                    password[i] = shuffledChars[randomBytes[i] % shuffledChars.Length];
                }

                // Şifre uzunluğunu kontrol et
                if (password.Distinct().Count() < 4)
                {
                    // Eğer şifrede en az 4 farklı karakter yoksa, yeniden oluştur
                    return GeneratePassword(length);
                }

                // Şifreyi döndür
                return new string(password);
            }
        }
    }
}