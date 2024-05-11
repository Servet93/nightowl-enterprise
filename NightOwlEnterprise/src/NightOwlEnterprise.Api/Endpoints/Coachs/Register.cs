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

namespace NightOwlEnterprise.Api.Endpoints.Coachs;

public static class Register
{
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();
    
    public static void MapRegister<TUser>(this IEndpointRouteBuilder endpoints, IEmailSender<ApplicationUser> emailSender, LinkGenerator linkGenerator)
        where TUser : class, new()
    {
        // NOTE: We cannot inject UserManager<TUser> directly because the TUser generic parameter is currently unsupported by RDG.
        // https://github.com/dotnet/aspnetcore/issues/47338
        endpoints.MapPost("/register", async Task<Results<Ok, ProblemHttpResult>>
            ([FromBody] CoachRegisterRequest registration, HttpContext context, [FromServices] IServiceProvider sp) =>
        {
            var identityErrors = new List<IdentityError>();

            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var errorDescriber = sp.GetRequiredService<TurkishIdentityErrorDescriber>();

            var name = registration.Name;
            var email = registration.Email;
            var phoneNumber = registration.PhoneNumber;
            var address = registration.Address;
            var city = registration.City;

            if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
            {
                identityErrors.Add(userManager.ErrorDescriber.InvalidEmail(email));
                //return userManager.ErrorDescriber.InvalidEmail(email).CreateProblem();
            }

            if (string.IsNullOrEmpty(name) || name.Length < 3) // :)
            {
                identityErrors.Add(errorDescriber.InvalidName(name));
                //return errorDescriber.InvalidName(name).CreateProblem();
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
                //return TurkishIdentityErrorDescriber.InvalidMobile(phoneNumber).CreateProblem();
                //return IdentityResult.Failed(errorDescriber.InvalidMobile(phoneNumber)).CreateValidationProblem();
            }

            if (string.IsNullOrEmpty(address))
            {
                identityErrors.Add(errorDescriber.RequiredAddress());
                //return errorDescriber.RequiredAddress().CreateProblem();
            }

            if (!CommonVariables.Cities.Contains(city))
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
                UserType = registration.CoachType == CoachType.Coach ? UserType.Coach : UserType.Pdr,
            };

            var result = await userManager.CreateAsync(user, registration.Password);

            if (!result.Succeeded)
            {
                identityErrors.AddRange(result.Errors);
                //return result.Errors.CreateProblem();
            }

            if (identityErrors.Any())
            {
                return identityErrors.CreateProblem("Koç kaydetme işlemi başarısız");
            }

            return TypedResults.Ok();
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachIdentity);
    }

    public sealed class CoachRegisterRequest
    {
        [DefaultValue("NightOwlCoach")]
        public required string Name { get; init; }
        
        [DefaultValue("coach-nightowl-enterprise@gmail.com")]
        public required string Email { get; init; }
        
        [DefaultValue("Aa.123456")]
        public required string Password { get; init; }
        
        [SwaggerSchema("The student's phone number", Description = "phone number format is 5xx-xxx-xxx-xxx")]
        [DefaultValue("533-333-33-33")]
        public required string PhoneNumber { get; init; }
        
        [DefaultValue("Başakşehir")]
        public required string Address { get; init; }
        
        [DefaultValue("İstanbul")]
        public required string City { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CoachType CoachType { get; set; }
    }

    public enum CoachType
    {
        Coach,
        Pdr
    }
    
    public class CoachRegisterRequestExamples : IMultipleExamplesProvider<CoachRegisterRequest>
    {
        public IEnumerable<SwaggerExample<CoachRegisterRequest>> GetExamples()
        {
            yield return SwaggerExample.Create("Servet,CoachType:Coach", new CoachRegisterRequest()
            {
                Name = "Servet", Address = "Bağcılar", City = "İstanbul", Email = "servet-coach@gmail.com",
                Password = "Aa123456", PhoneNumber = "533-333-33-33", CoachType = CoachType.Coach
            });
            
            yield return SwaggerExample.Create("Servet,CoachType:Pdr", new CoachRegisterRequest()
            {
                Name = "Servet", Address = "Bağcılar", City = "İstanbul", Email = "servet-pdr@gmail.com",
                Password = "Aa123456", PhoneNumber = "533-333-33-33", CoachType = CoachType.Pdr
            });
            
            yield return SwaggerExample.Create("Burak,Package:Coach", new CoachRegisterRequest()
            {
                Name = "Burak", Address = "Güngören", City = "İstanbul", Email = "burak-coach@gmail.com",
                Password = "Aa123456", PhoneNumber = "533-333-33-33", CoachType = CoachType.Coach,
            });
            
            yield return SwaggerExample.Create("Burak,Package:Pdr", new CoachRegisterRequest()
            {
                Name = "Burak", Address = "Güngören", City = "İstanbul", Email = "burak-pdr@gmail.com",
                Password = "Aa123456", PhoneNumber = "533-333-33-33", CoachType = CoachType.Pdr,
            });
            yield return SwaggerExample.Create("Eren,Package:Coach", new CoachRegisterRequest()
            {
                Name = "Eren", Address = "Maltepe", City = "İstanbul", Email = "eren-koc@gmail.com",
                Password = "Aa123456", PhoneNumber = "533-333-33-33", CoachType = CoachType.Coach,
            });
            
            yield return SwaggerExample.Create("Eren,Package:Pdr", new CoachRegisterRequest()
            {
                Name = "Eren", Address = "Maltepe", City = "İstanbul", Email = "eren-pdr@gmail.com",
                Password = "Aa123456", PhoneNumber = "533-333-33-33", CoachType = CoachType.Pdr,
            });
            yield return SwaggerExample.Create("Turgay,Package:Coach", new CoachRegisterRequest()
            {
                Name = "Turgay", Address = "Maltepe", City = "İstanbul", Email = "turgay-koc@gmail.com",
                Password = "Aa123456", PhoneNumber = "533-333-33-33", CoachType = CoachType.Coach,
            });
            yield return SwaggerExample.Create("Turgay,Package:Pdr", new CoachRegisterRequest()
            {
                Name = "Turgay", Address = "Maltepe", City = "İstanbul", Email = "turgay-pdr@gmail.com",
                Password = "Aa123456", PhoneNumber = "533-333-33-33", CoachType = CoachType.Pdr,
            });
        }
    }
}