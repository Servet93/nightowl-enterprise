using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Stripe;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public static class Register
{
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();
    
    public static void MapRegister<TUser>(this IEndpointRouteBuilder endpoints, IEmailSender<ApplicationUser> emailSender, LinkGenerator linkGenerator)
        where TUser : class, new()
    {
        // NOTE: We cannot inject UserManager<TUser> directly because the TUser generic parameter is currently unsupported by RDG.
        // https://github.com/dotnet/aspnetcore/issues/47338
        endpoints.MapPost("/register", async Task<Results<Ok, ProblemHttpResult>>
            ([FromBody] StudentRegisterRequest registration, HttpContext context, [FromServices] IServiceProvider sp) =>
        {
            var identityErrors = new List<IdentityError>();
            
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var errorDescriber = sp.GetRequiredService<TurkishIdentityErrorDescriber>();
            var studentEmailSender = sp.GetRequiredService<StudentEmailSender>();
            
            if (!userManager.SupportsUserEmail)
            {
                throw new NotSupportedException($"{nameof(MapRegister)} requires a user store with email support.");
            }
            
            var name = registration.Name;
            var email = registration.Email;
            var phoneNumber = registration.PhoneNumber;
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

            //533-222-88-44
            if (string.IsNullOrEmpty(phoneNumber) ||
                phoneNumber.Length != 13 ||
                phoneNumber.Split("-").Length != 4 ||
                phoneNumber.Split("-")[0].First() != '5' ||
                !int.TryParse(phoneNumber.Split("-")[0], out var parseResult) ||
                !int.TryParse(phoneNumber.Split("-")[1], out parseResult) ||
                !int.TryParse(phoneNumber.Split("-")[2], out parseResult) || 
                !int.TryParse(phoneNumber.Split("-")[3], out parseResult))
            {
                identityErrors.Add(TurkishIdentityErrorDescriber.InvalidMobile(phoneNumber));
                //return IdentityResult.Failed(errorDescriber.InvalidMobile(phoneNumber)).CreateValidationProblem();
            }

            if (string.IsNullOrEmpty(address))
            {
                identityErrors.Add(errorDescriber.RequiredAddress());
                //return IdentityResult.Failed(errorDescriber.RequiredAddress()).CreateValidationProblem();
            }
            
            if (!CommonVariables.Cities.Contains(city))
            {
                identityErrors.Add(errorDescriber.InvalidCity(city));
                //return IdentityResult.Failed(errorDescriber.InvalidCity(city)).CreateValidationProblem();
            }
            
            if (identityErrors.Any())
            {
                return identityErrors.CreateProblem("Öğrenci kaydetme işlemi başarısız");
            }

            var userName = email.Split('@')[0];

            var dt = DateTime.UtcNow;
            
            var user = new ApplicationUser()
            {
                Name = name,
                UserName = userName,
                Email = email,
                Address = address,
                City = city,
                PhoneNumber = phoneNumber,
                UserType = UserType.Student,
                SubscriptionHistories = new List<SubscriptionHistory>()
                {
                    new SubscriptionHistory()
                    {
                        Type = registration.SubscriptionType,
                        SubscriptionId = "test-sub_1PCHn7Gw8vCPSCnNvqs5FoVD",
                        SubscriptionState = "active",
                        SubscriptionStartDate = dt,
                        SubscriptionEndDate = dt.AddMonths(1),
                        InvoiceId = "test-in_1PCHn7Gw8vCPSCnN8AoUDZhb",
                        InvoiceState = "paid",
                        LastError = String.Empty
                    }
                },
                StudentDetail = new StudentDetail()
                {
                    Status = StudentStatus.OnboardProgress,
                }
            };
            
            var result = await userManager.CreateAsync(user, registration.Password);
            
            if (!result.Succeeded)
            {
                identityErrors.AddRange(result.Errors);
                //return result.CreateValidationProblem();
            }
            
            // await SendConfirmationEmailAsync(user, userManager, context, email);

            if (identityErrors.Any())
            {
                return identityErrors.CreateProblem("Öğrenci kaydetme işlemi başarısız");
            }
            
            return TypedResults.Ok();
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.StudentsIdentity);
        
        async Task SendConfirmationEmailAsync(ApplicationUser user, UserManager<ApplicationUser> userManager, HttpContext context, string email, bool isChange = false)
        {
            if (ConfirmEmail.confirmEmailEndpointName is null)
            {
                throw new NotSupportedException("No email confirmation endpoint was registered!");
            }

            var code = isChange
                ? await userManager.GenerateChangeEmailTokenAsync(user, email)
                : await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var userId = await userManager.GetUserIdAsync(user);
            var routeValues = new RouteValueDictionary()
            {
                ["userId"] = userId,
                ["code"] = code,
            };

            if (isChange)
            {
                // This is validated by the /confirmEmail endpoint on change.
                routeValues.Add("changedEmail", email);
            }

            var confirmEmailUrl = linkGenerator.GetUriByName(context, ConfirmEmail.confirmEmailEndpointName, routeValues)
                                  ?? throw new NotSupportedException($"Could not find endpoint named '{ConfirmEmail.confirmEmailEndpointName}'.");

            await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(confirmEmailUrl));
        }
    }

    public sealed class StudentRegisterRequest
    {
        [DefaultValue("NightOwl")]
        public required string Name { get; init; }
        
        [DefaultValue("nightowl-enterprise@gmail.com")]
        public required string Email { get; init; }
        
        [DefaultValue("Aa.123456")]
        public required string Password { get; init; }
        
        public required string Address { get; init; }
        
        public required string City { get; init; }
        
        [SwaggerSchema("The student's phone number", Description = "phone number format is 5xx-xxx-xxx-xxx")]
        [DefaultValue("533-333-33-33")]
        public required string PhoneNumber { get; init; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required SubscriptionType SubscriptionType { get; init; }
    }
    public class StudentRegisterRequestExamples : IMultipleExamplesProvider<StudentRegisterRequest>
    {
        public IEnumerable<SwaggerExample<StudentRegisterRequest>> GetExamples()
        {
            yield return SwaggerExample.Create("Servet,Package:Coach", new StudentRegisterRequest()
            {
                Name = "Servet", Address = "Bağcılar", City = "İstanbul", Email = "servet-package-coach@gmail.com",
                Password = "Aa123456", PhoneNumber = "533-333-33-33", SubscriptionType = SubscriptionType.Coach,
            });
            
            yield return SwaggerExample.Create("Servet,Package:Pdr", new StudentRegisterRequest()
            {
                Name = "Servet", Address = "Bağcılar", City = "İstanbul", Email = "servet-package-pdr@gmail.com",
                Password = "Aa123456", PhoneNumber = "533-333-33-33", SubscriptionType = SubscriptionType.Pdr,
            });
            
            yield return SwaggerExample.Create("Burak,Package:Coach", new StudentRegisterRequest()
            {
                Name = "Burak", Address = "Güngören", City = "İstanbul", Email = "burak-package-coach@gmail.com",
                Password = "Aa123456", PhoneNumber = "533-333-33-33", SubscriptionType = SubscriptionType.Coach,
            });
            
            yield return SwaggerExample.Create("Burak,Package:Pdr", new StudentRegisterRequest()
            {
                Name = "Burak", Address = "Güngören", City = "İstanbul", Email = "burak-package-pdr@gmail.com",
                Password = "Aa123456", PhoneNumber = "533-333-33-33", SubscriptionType = SubscriptionType.Pdr,
            });
            yield return SwaggerExample.Create("Eren,Package:Coach", new StudentRegisterRequest()
            {
                Name = "Eren", Address = "Maltepe", City = "İstanbul", Email = "eren-package-coach@gmail.com",
                Password = "Aa123456", PhoneNumber = "533-333-33-33", SubscriptionType = SubscriptionType.Coach,
            });
            
            yield return SwaggerExample.Create("Eren,Package:Pdr", new StudentRegisterRequest()
            {
                Name = "Eren", Address = "Maltepe", City = "İstanbul", Email = "eren-package-pdr@gmail.com",
                Password = "Aa123456", PhoneNumber = "533-333-33-33", SubscriptionType = SubscriptionType.Pdr,
            });
            yield return SwaggerExample.Create("Turgay,Package:Coach", new StudentRegisterRequest()
            {
                Name = "Turgay", Address = "Maltepe", City = "İstanbul", Email = "turgay-package-coach@gmail.com",
                Password = "Aa123456", PhoneNumber = "533-333-33-33", SubscriptionType = SubscriptionType.Coach,
            });
            yield return SwaggerExample.Create("Turgay,Package:Pdr", new StudentRegisterRequest()
            {
                Name = "Turgay", Address = "Maltepe", City = "İstanbul", Email = "turgay-package-pdr@gmail.com",
                Password = "Aa123456", PhoneNumber = "533-333-33-33", SubscriptionType = SubscriptionType.Pdr,
            });
        }
    }
}