using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO.Pipelines;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        endpoints.MapPost("/payment", async Task<Results<Ok<ConfirmIntentResult>, ProblemHttpResult>>
            ([FromBody] StudentRegisterRequestWithPaymentMethodId registration, HttpContext context, [FromServices] IServiceProvider sp) =>
        {
            var identityErrors = new List<IdentityError>();
            
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
                identityErrors.Add(userManager.ErrorDescriber.InvalidEmail(email));
                //return IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)).CreateValidationProblem();
            }
            
            if (string.IsNullOrEmpty(name) || name.Length < 3) // :)
            {
                identityErrors.Add(errorDescriber.InvalidName(name));
                //return IdentityResult.Failed(errorDescriber.InvalidName(name)).CreateValidationProblem();
            }

            if (string.IsNullOrEmpty(address))
            {
                identityErrors.Add(errorDescriber.RequiredAddress());
                //return IdentityResult.Failed(errorDescriber.RequiredAddress()).CreateValidationProblem();
            }

            if (!cities.Contains(city))
            {
                identityErrors.Add(errorDescriber.InvalidCity(city));
                //return IdentityResult.Failed(errorDescriber.InvalidCity(city)).CreateValidationProblem();
            }

            var userName = email.Split('@')[0];
            
            var user = new ApplicationUser()
            {
                Name = name,
                UserName = userName,
                Email = email,
                Address = address,
                City = city,
                UserType = UserType.Student,
                StudentDetail = new StudentDetail()
                {
                    Status = StudentStatus.PaymentAwaiting,
                }
            };
            
            var result = await userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                identityErrors.AddRange(result.Errors);
                //return result.CreateValidationProblem();
            }

            if (identityErrors.Any())
            {
                return identityErrors.CreateProblem();
            }
            
            var confirmPaymentResult = ConfirmPayment(user.Id, registration.PaymentMethodId, context);

            if (!string.IsNullOrEmpty(confirmPaymentResult.error))
            {
                await userManager.DeleteAsync(user);

                var errorDesciptor = new ErrorDescriptor("PaymentIntentCreationError", confirmPaymentResult.error);

                return errorDesciptor.CreateProblem();
                
                // return TypedResults.ValidationProblem(new Dictionary<string, string[]>()
                // {
                //     { "PaymentIntentCreationError", new string[] {confirmPaymentResult.error}},
                // });
            }
            
            return TypedResults.Ok(confirmPaymentResult);
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags("Öğrenci");
        
        
        endpoints.MapPost("/subscribe", async Task<Results<Ok<ConfirmIntentResult>, ProblemHttpResult>>
            ([FromBody] StudentRegisterRequestWithPaymentMethodId registration, HttpContext context, [FromServices] IServiceProvider sp) =>
        {
            var identityErrors = new List<IdentityError>();

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
                identityErrors.Add(userManager.ErrorDescriber.InvalidEmail(email));
                //return IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)).CreateValidationProblem();
            }
            
            if (string.IsNullOrEmpty(name) || name.Length < 3) // :)
            {
                identityErrors.Add(errorDescriber.InvalidName(name));
                //return IdentityResult.Failed(errorDescriber.InvalidName(name)).CreateValidationProblem();
            }

            if (string.IsNullOrEmpty(address))
            {
                identityErrors.Add(errorDescriber.RequiredAddress());
                //return IdentityResult.Failed(errorDescriber.RequiredAddress()).CreateValidationProblem();
            }

            if (!cities.Contains(city))
            {
                identityErrors.Add(errorDescriber.InvalidCity(city));
                //return IdentityResult.Failed(errorDescriber.InvalidCity(city)).CreateValidationProblem();
            }

            var userName = email.Split('@')[0];
            
            var user = new ApplicationUser()
            {
                Name = name,
                UserName = userName,
                Email = email,
                Address = address,
                City = city,
                UserType = UserType.Student,
                StudentDetail = new StudentDetail()
                {
                    Status = StudentStatus.PaymentAwaiting, 
                }
            };
            
            var result = await userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                identityErrors.AddRange(result.Errors);
                //return result.CreateValidationProblem();
            }

            if (identityErrors.Any())
            {
                return identityErrors.CreateProblem("Öğrenci kaydetme işlemi başarısız.");
            }

            var userId = user.Id;
            
            var createCustomerResult = CreateCustomer(user);

            if (!createCustomerResult.Item1)
            {
                await userManager.DeleteAsync(user);

                var error = new ErrorDescriptor("CreateCustomerOnStripeError", createCustomerResult.Item2);

                return error.CreateProblem("Öğrenci kaydetme işlemi başarısız.");
                
                // return TypedResults.ValidationProblem(new Dictionary<string, string[]>()
                // {
                //     { "CreateCustomerOnStripeError", new string[] {createCustomerResult.Item2}},
                // });
            }

            var customerId = createCustomerResult.Item3.Id;

            user.CustomerId = customerId;
            
            await userManager.UpdateAsync(user);
            
            var createSubscriptionResult = CreateSubscription(user.Email, customerId, "price_1OvFmMEyxtA03PfNmgnvxwSu",
                user.Id, registration.PaymentMethodId, context);

            if (!createSubscriptionResult.Item1)
            {
                await userManager.DeleteAsync(user);
             
                var error = new ErrorDescriptor("CreateSubscriptionOnStripeError", createSubscriptionResult.Item2);

                return error.CreateProblem("Öğrenci kaydetme işlemi başarısız.");
                
                // return TypedResults.ValidationProblem(new Dictionary<string, string[]>()
                // {
                //     { "CreateSubscriptionOnStripeError", new string[] {createSubscriptionResult.Item2}},
                // });
            }

            return TypedResults.Ok(createSubscriptionResult.Item3);
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags("Öğrenci");

        // const string stripeCredentialSigningSecret = "whsec_7YBhw4M9aSypQq7K6fVCJnhcbduPb6yN";

        endpoints.MapPost("/payment-webhook", async (HttpContext httpContext, [FromServices] IServiceProvider sp) =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            var logger = loggerFactory.CreateLogger("Payment-Webhook");

            try
            {

                var json = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();

                var stripeEvent = EventUtility.ConstructEvent(json,
                    httpContext.Request.Headers["Stripe-Signature"], stripeCredentialSigningSecret);

                if (stripeEvent?.Data?.Object is null)
                {
                    logger.LogWarning("StripeEvent object is empty");
                    return Results.Empty;
                }

                var customerService = new CustomerService();
                Customer customer = null;
                ApplicationUser user = null;
                PaymentIntent intent = null;
                Invoice invoice = null;
                string userId;

                var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":

                        intent = (PaymentIntent)stripeEvent.Data.Object;
                        
                        logger.LogInformation(
                            "Payment Succeeded. PaymentIntentId: {PaymentIntentId}, CustomerId: {CustomerId}",
                            intent.Id, intent.CustomerId);

                        customer = customerService.Get(intent.CustomerId);

                        customer.Metadata?.TryGetValue("UserId", out userId);

                        user = await GetUser(logger, dbContext, userManager, customer.Metadata);

                        if (user is null)
                        {
                            logger.LogWarning("User not found. ReceiptEmail: {ReceiptEmail}", intent.ReceiptEmail);
                            return TypedResults.Empty;
                        }

                        var generatedPassword = PasswordGenerator.GeneratePassword(8);

                        var addPasswordAsyncResult = await userManager.AddPasswordAsync(user!, generatedPassword);

                        if (!addPasswordAsyncResult.Succeeded)
                        {
                            var descriptions = addPasswordAsyncResult.Errors.Select(x => x.Description);
                            var errMsg = string.Join(",", descriptions);
                            logger.LogWarning(
                                "User password couldn't added. PaymentIntentId: {PaymentIntentId}, CustomerId: {CustomerId}, UserId: {UserId}, Message: {Message}",
                                intent.Id, intent.CustomerId, user.Id, errMsg);

                            return TypedResults.Empty;
                        }

                        var studentEmailSender = sp.GetRequiredService<StudentEmailSender>();

                        user.StudentDetail.Status = StudentStatus.Active;

                        var updateAsyncResult = await userManager.UpdateAsync(user);

                        if (!updateAsyncResult.Succeeded)
                        {
                            var descriptions = updateAsyncResult.Errors.Select(x => x.Description);
                            var errMsg = string.Join(",", descriptions);
                            logger.LogWarning(
                                "User couldn't updated. PaymentIntentId: {PaymentIntentId}, CustomerId: {CustomerId}, UserId: {UserId}, Message: {Message}",
                                intent.Id, user.CustomerId, user.Id,
                                errMsg);

                            return TypedResults.Empty;
                        }

                        await studentEmailSender.SendSignInInfo(user!, generatedPassword);

                        break;
                    case "payment_intent.payment_failed":

                        intent = (PaymentIntent)stripeEvent.Data.Object;

                        logger.LogInformation(
                            "Payment Failure. PaymentIntentId: {PaymentIntentId}, CustomerId: {CustomerId}", intent.Id,
                            intent.CustomerId);

                        customer = customerService.Get(intent.CustomerId);

                        customer.Metadata?.TryGetValue("UserId", out userId);

                        user = await GetUser(logger, dbContext, userManager, customer.Metadata);

                        if (user is null)
                        {
                            logger.LogWarning("User not found. ReceiptEmail: {ReceiptEmail}", intent.ReceiptEmail);
                            return TypedResults.Empty;
                        }

                        // Notify the customer that payment failed

                        var deleteAsyncResult = await userManager.DeleteAsync(user!);

                        if (!deleteAsyncResult.Succeeded)
                        {
                            var descriptions = deleteAsyncResult.Errors.Select(x => x.Description);
                            var errMsg = string.Join(",", descriptions);
                            logger.LogWarning(
                                "User couldn't deleted. PaymentIntentId: {PaymentIntentId}, UserId: {UserId}, Message: {Message}",
                                intent, user.Id, errMsg);
                        }

                        break;
                    case "invoice.paid":
                        // Used to provision services after the trial has ended.
                        // The status of the invoice will show up as paid. Store the status in your
                        // database to reference when a user accesses your service to avoid hitting rate
                        // limits.
                        invoice = (Invoice)stripeEvent.Data.Object;

                        customer = customerService.Get(invoice.CustomerId);

                        customer.Metadata.TryGetValue("UserId", out userId);
                        
                        logger.LogInformation(
                            "Invoice Paid. SubscriptionId: {SubscriptionId}, InvoiceId: {InvoiceId}, CustomerId: {CustomerId}, UserId: {UserId}",
                            invoice.SubscriptionId, invoice.Id, invoice.CustomerId, userId);

                        user = await GetUser(logger, dbContext, userManager, customer.Metadata);

                        if (user is null)
                        {
                            return TypedResults.Empty;
                        }

                        user.StudentDetail.Status = StudentStatus.Active;
                        user.SubscriptionId = invoice.SubscriptionId;
                        
                        await userManager.UpdateAsync(user);

                        break;

                    case "invoice.payment_failed":
                        // Used to provision services after the trial has ended.
                        // The status of the invoice will show up as paid. Store the status in your
                        // database to reference when a user accesses your service to avoid hitting rate
                        // limits.
                        invoice = (Invoice)stripeEvent.Data.Object;

                        customer = customerService.Get(invoice.CustomerId);

                        customer.Metadata.TryGetValue("UserId", out userId);

                        logger.LogInformation(
                            "Invoice Payment Failed. SubscriptionId: {SubscriptionId}, InvoiceId: {InvoiceId}, CustomerId: {CustomerId}, UserId: {UserId}",
                            invoice.SubscriptionId, invoice.Id, invoice.CustomerId, userId);

                        user = await GetUser(logger, dbContext, userManager, customer.Metadata);

                        if (user is null)
                        {
                            return TypedResults.Empty;
                        }

                        user.StudentDetail.Status = StudentStatus.PaymentAwaiting;

                        await userManager.UpdateAsync(user);

                        break;

                    case "customer.subscription.deleted":
                        // Used to provision services after the trial has ended.
                        // The status of the invoice will show up as paid. Store the status in your
                        // database to reference when a user accesses your service to avoid hitting rate
                        // limits.


                        break;
                    default:
                        // Handle other event types

                        break;
                }

                return TypedResults.Empty;

            }
            catch (StripeException e)
            {
                logger.LogError(e, "StripeException - Payment Webhook has an error");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception - Payment Webhook has an error");
            }

            return TypedResults.Empty;
        }).WithOpenApi().WithTags("Öğrenci");
    }

    private static async Task<ApplicationUser> GetUser(ILogger logger, ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, Dictionary<string, string> metadata)
    {
        string strUserId = null;
        
        var isExist = metadata?.TryGetValue("UserId", out strUserId) ?? false;

        ApplicationUser user = null;
                
        if (isExist)
        {
            Guid.TryParse(strUserId, out var userId);

            user = dbContext.Users.Include(x => x.StudentDetail).FirstOrDefault(x => x.Id == userId);

            if (user is null)
            {
                logger.LogWarning("User not found. UserId: {UserId}", userId);
            }
                    
        }

        return user;
    }
    
    private static (bool, string, Customer) CreateCustomer(ApplicationUser user)
    {
        var errorMessage = string.Empty;
        
        try
        {
            var options = new CustomerCreateOptions
            {
                Email = user.Email,
                Name = user.Name,
                Shipping = new ShippingOptions
                {
                    Address = new AddressOptions
                    {
                        City = user.City,
                        Country = "TR",
                        Line1 = user.Address,
                        // PostalCode = "97712",
                        // State = "CA",
                    },
                    Name = user.Name,
                },
                Address = new AddressOptions
                {
                    City = user.City,
                    Country = "TR",
                    Line1 = user.Address,
                    // PostalCode = "97712",
                    // State = "CA",
                },
                Metadata = new Dictionary<string, string>()
                {
                    {"UserId", user.Id.ToString()}
                }
            };
            var service = new CustomerService();
            var customer = service.Create(options);
            return (true, errorMessage, customer);
        }
        catch (Exception e)
        {
            errorMessage = e.Message;
        }
        return (false, errorMessage, null);
    }

    private static (bool, string, ConfirmIntentResult) CreateSubscription(string email, string customerId, string priceId, Guid userId, string paymentMethodId, HttpContext httpContext)
    {
        var errorMessage = string.Empty;
        
        try
        {
            // Automatically save the payment method to the subscription
            // when the first payment is successful.
            var paymentSettings = new SubscriptionPaymentSettingsOptions {
                SaveDefaultPaymentMethod = "on_subscription",
            };

            var subscriptionOptions = new SubscriptionCreateOptions
            {
                Customer = customerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = priceId,
                    },
                },
                PaymentSettings = paymentSettings,
                PaymentBehavior = "default_incomplete",
                Metadata = new Dictionary<string, string>()
                {
                    {"UserId", userId.ToString()}
                }
            };
            subscriptionOptions.AddExpand("latest_invoice.payment_intent");
            subscriptionOptions.AddExpand("pending_setup_intent");
            var subscriptionService = new SubscriptionService();
            try
            {
                Subscription subscription = subscriptionService.Create(subscriptionOptions);

                var paymentIntentService = new PaymentIntentService();
                var paymentIntentConfirmOptions = new PaymentIntentConfirmOptions()
                {
                    PaymentMethod = paymentMethodId,
                    ReceiptEmail = email,
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
                    }
                };
                paymentIntentConfirmOptions.AddExpand("customer");
                var paymentIntentResult = paymentIntentService.Confirm(subscription.LatestInvoice.PaymentIntent.Id,
                    paymentIntentConfirmOptions);

                return (true, string.Empty, new ConfirmIntentResult(paymentIntentResult.ClientSecret,
                    paymentIntentResult.Status,
                    string.Empty));
            }
            catch (StripeException e)
            {
                errorMessage = e.Message;
            }
        }
        catch (Exception e)
        {
            errorMessage = e.Message;
        }

        return (false, errorMessage, null);
    }

    record SubscriptionCreateResponse(string Type, string ClientSecret);
    
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