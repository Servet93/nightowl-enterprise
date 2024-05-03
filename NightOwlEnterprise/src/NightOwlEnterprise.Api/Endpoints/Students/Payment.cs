using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO.Pipelines;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public static class Payment
{
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();
    
    public static void MapPayment<TUser>(this IEndpointRouteBuilder endpoints, StripeCredential stripeCredential)
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

            var dbContext = sp.GetRequiredService<ApplicationDbContext>();
            UserManager<ApplicationUser?> userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
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
            
            if (identityErrors.Any())
            {
                return identityErrors.CreateProblem("Öğrenci kaydetme işlemi başarısız");
            }

            ApplicationUser? user = null;
            
            user = dbContext.Users.FirstOrDefault(x => x.Email == email);

            if (user is null)
            {
                var userName = email.Split('@')[0];
            
                user = new ApplicationUser()
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
                
            }

            var priceId = registration.SubscriptionType == SubscriptionType.Coach
                ? stripeCredential.DereceliKocPriceId
                : stripeCredential.PdrPriceId;            
            
            var createSubscriptionResult = CreateSubscription(user.Email, user.CustomerId, priceId,
                user.Id, registration.PaymentMethodId, context);

            if (!createSubscriptionResult.Item1)
            {
                // await userManager.DeleteAsync(user);
             
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

            var stripeEventType = "Common";
            
            try
            {
                var json = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();

                var stripeEvent = EventUtility.ConstructEvent(json,
                    httpContext.Request.Headers["Stripe-Signature"], stripeCredential.SigningSecret);

                if (stripeEvent?.Data?.Object is null)
                {
                    logger.LogWarning("StripeEvent object is empty");
                    return Results.Empty;
                }

                var customerService = new CustomerService();
                var subscriptionService = new SubscriptionService();
                var invoiceService = new InvoiceService();
                var paymentIntentService = new PaymentIntentService();
                
                Customer customer = null;
                Subscription subscription = null;
                ApplicationUser user = null;
                PaymentIntent paymentIntent = null;
                Invoice invoice = null;
                string strUserId = null;
                Guid userId;
                SubscriptionType? subscriptionType = null;
                
                var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                stripeEventType = stripeEvent.Type;
                
                switch (stripeEvent.Type)
                {
                    case "invoice.paid":
                        // Used to provision services after the trial has ended.
                        // The status of the invoice will show up as paid. Store the status in your
                        // database to reference when a user accesses your service to avoid hitting rate
                        // limits.
                        invoice = (Invoice)stripeEvent.Data.Object;

                        //customer = customerService.Get(invoice.CustomerId);
                        
                        invoice.SubscriptionDetails.Metadata.TryGetValue("UserId", out strUserId);

                        Guid.TryParse(strUserId, out userId);
                        // customer.Metadata.TryGetValue("UserId", out strUserId);
                        
                        logger.LogInformation(
                            "Invoice Paid. SubscriptionId: {SubscriptionId}, InvoiceId: {InvoiceId}, CustomerId: {CustomerId}, UserId: {UserId}",
                            invoice.SubscriptionId, invoice.Id, invoice.CustomerId, strUserId);

                        user = await GetUser(logger, dbContext, userManager, invoice.SubscriptionDetails.Metadata);

                        if (user is null)
                        {
                            return TypedResults.Empty;
                        }

                        var subscriptionPriceId = invoice.Lines.FirstOrDefault()?.Price.Id;
                        
                        subscription = subscriptionService.Get(invoice.SubscriptionId);
                        
                        subscriptionType = subscriptionPriceId == stripeCredential.DereceliKocPriceId
                            ? SubscriptionType.Coach
                            : SubscriptionType.Pdr;
                        
                        //Hesap oluşturulmuş password atanmamış
                        if (string.IsNullOrEmpty(user.PasswordHash))
                        {
                            var generatedPassword = PasswordGenerator.GeneratePassword(8);

                            var addPasswordAsyncResult = await userManager.AddPasswordAsync(user!, generatedPassword);
                            
                            if (!addPasswordAsyncResult.Succeeded)
                            {
                                var descriptions = addPasswordAsyncResult.Errors.Select(x => x.Description);
                                var errMsg = string.Join(",", descriptions);
                                logger.LogWarning(
                                    "User password couldn't added. PaymentIntentId: {PaymentIntentId}, CustomerId: {CustomerId}, UserId: {UserId}, Message: {Message}",
                                    paymentIntent.Id, paymentIntent.CustomerId, user.Id, errMsg);

                                return TypedResults.Empty;
                            }
                            
                            var studentEmailSender = sp.GetRequiredService<StudentEmailSender>();

                            user.StudentDetail.Status = StudentStatus.OnboardProgress;

                            var updateAsyncResult = await userManager.UpdateAsync(user);

                            if (!updateAsyncResult.Succeeded)
                            {
                                var descriptions = updateAsyncResult.Errors.Select(x => x.Description);
                                var errMsg = string.Join(",", descriptions);
                                logger.LogWarning(
                                    "User couldn't updated. PaymentIntentId: {PaymentIntentId}, CustomerId: {CustomerId}, UserId: {UserId}, Message: {Message}",
                                    paymentIntent.Id, user.CustomerId, user.Id,
                                    errMsg);

                                return TypedResults.Empty;
                            }

                            await studentEmailSender.SendSignInInfo(user!, generatedPassword);
                        }
                        
                        var subscriptionHistory = dbContext.SubscriptionHistories
                            .FirstOrDefault(x => x.UserId == userId && x.SubscriptionId == invoice.SubscriptionId && x.InvoiceId == invoice.Id);
                        
                        if (subscriptionHistory is not null)
                        {
                            subscriptionHistory.InvoiceState = invoice.Status;
                            subscriptionHistory.SubscriptionState = subscription.Status;
                            subscriptionHistory.SubscriptionStartDate = subscription.CurrentPeriodStart;
                            subscriptionHistory.SubscriptionEndDate = subscription.CurrentPeriodEnd;
                            subscriptionHistory.Type = subscriptionType.Value;
                            subscriptionHistory.LastError = string.Empty;
                            
                            user.StudentDetail.Status = StudentStatus.Active;
                        }
                        else
                        {
                            dbContext.SubscriptionHistories.Add(new SubscriptionHistory()
                            {
                                UserId = userId,
                                InvoiceId = invoice.Id,
                                SubscriptionId = invoice.SubscriptionId,
                                InvoiceState = invoice.Status,
                                SubscriptionState = subscription.Status,
                                SubscriptionStartDate = subscription.CurrentPeriodStart,
                                SubscriptionEndDate = subscription.CurrentPeriodEnd,
                                Type = subscriptionType.Value,
                                LastError = string.Empty,
                            });    
                        }

                        await dbContext.SaveChangesAsync();

                        break;

                    case "invoice.payment_failed":
                        // Used to provision services after the trial has ended.
                        // The status of the invoice will show up as paid. Store the status in your
                        // database to reference when a user accesses your service to avoid hitting rate
                        // limits.
                        invoice = (Invoice)stripeEvent.Data.Object;

                        // customer = customerService.Get(invoice.CustomerId);
                        //
                        // customer.Metadata.TryGetValue("UserId", out strUserId);
                        
                        invoice.SubscriptionDetails.Metadata.TryGetValue("UserId", out strUserId);

                        Guid.TryParse(strUserId, out userId);

                        logger.LogInformation(
                            "Invoice Payment Failed. SubscriptionId: {SubscriptionId}, InvoiceId: {InvoiceId}, CustomerId: {CustomerId}, UserId: {UserId}",
                            invoice.SubscriptionId, invoice.Id, invoice.CustomerId, strUserId);

                        user = await GetUser(logger, dbContext, userManager, invoice.SubscriptionDetails.Metadata);

                        if (user is null)
                        {
                            return TypedResults.Empty;
                        }
                        
                        // Notify the customer that payment failed
                        // Hesap ilk oluşturmada bir şekilde ödeme alınamazsa kullanıcıyı sil. 
                        // Diyelimki hesap oluşturuldu 1 ay kullanıldı ödeme günü geldi.Mail gönderildi ve ödeme isteniyor
                        //Ödeme alamadığında islinmemesi için password ataması yapılıp yapılmadığına bakıyoruz
                        //Yapılmamışsa ilk kayıt aşamasıdır diyebiliriz
                        
                        subscriptionPriceId = invoice.Lines.FirstOrDefault()?.Price.Id;
                    
                        //subscription = subscriptionService.Get(invoice.SubscriptionId);
                    
                        subscriptionType = subscriptionPriceId == stripeCredential.DereceliKocPriceId
                            ? SubscriptionType.Coach
                            : SubscriptionType.Pdr;
                        
                        subscription = subscriptionService.Get(invoice.SubscriptionId);

                        var _subscriptionHistory = dbContext.SubscriptionHistories
                            .FirstOrDefault(x =>
                                x.UserId == userId && x.SubscriptionId == invoice.SubscriptionId &&
                                x.InvoiceId == invoice.Id);

                        paymentIntent = paymentIntentService.Get(invoice.PaymentIntentId);
                        
                        // subscriptionType =
                        //     subscription.Items.FirstOrDefault().Price.Id == stripeCredential.DereceliKocPriceId
                        //         ? SubscriptionType.Coach
                        //         : SubscriptionType.Pdr;

                        var lastError = string.Empty;

                        if (paymentIntent?.LastPaymentError is not null)
                        {
                            lastError =
                                $"{paymentIntent.LastPaymentError.Code}, {paymentIntent.LastPaymentError.Message}, {paymentIntent.LastPaymentError.ErrorDescription}";
                        }else if (paymentIntent?.Status is not null)
                        {
                            lastError = paymentIntent.Status;
                        }
                        
                        if (_subscriptionHistory is null)
                        {
                            dbContext.SubscriptionHistories.Add(new SubscriptionHistory()
                            {
                                UserId = userId,
                                InvoiceId = invoice.Id,
                                SubscriptionId = invoice.SubscriptionId,
                                InvoiceState = invoice.Status,
                                SubscriptionState = subscription.Status,
                                SubscriptionStartDate = subscription.CurrentPeriodStart,
                                SubscriptionEndDate = subscription.CurrentPeriodEnd,
                                Type = subscriptionType.Value,
                                LastError = lastError
                            });
                        }
                        else
                        {
                            _subscriptionHistory.LastError = lastError;
                            _subscriptionHistory.SubscriptionState = subscription.Status;
                            _subscriptionHistory.InvoiceState = invoice.Status;
                        }

                        user.StudentDetail.Status = StudentStatus.PaymentAwaiting;

                        await dbContext.SaveChangesAsync();
                        
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
                logger.LogError(e, "StripeException - Payment Webhook has an error, {StripeEventType}", stripeEventType);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception - Payment Webhook has an error, {StripeEventType}", stripeEventType);
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

            user = dbContext.Users
                .Include(x => x.StudentDetail)
                .FirstOrDefault(x => x.Id == userId);

            if (user is null)
            {
                logger.LogWarning("User not found. UserId: {UserId}", userId);
            }
                    
        }

        return user;
    }
    
    private static (bool, string, Customer) CreateCustomer(ApplicationUser? user)
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
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required SubscriptionType SubscriptionType { get; init; }
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