using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Stripe;
using Swashbuckle.AspNetCore.Annotations;

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
            
            if (!Common.Cities.Contains(city))
            {
                identityErrors.Add(errorDescriber.InvalidCity(city));
                //return IdentityResult.Failed(errorDescriber.InvalidCity(city)).CreateValidationProblem();
            }

            if (registration.Tm == false &&
                registration.Mf == false &&
                registration.Sozel == false &&
                registration.Dil == false &&
                registration.Tyt == false)
            {
                var requiredExamTypeError = CommonErrorDescriptor.RequiredExamTypes();
                identityErrors.Add(new IdentityError()
                    { Code = requiredExamTypeError.Code, Description = requiredExamTypeError.Description });
            }

            var userName = email.Split('@')[0];
            
            var user = new ApplicationUser()
            {
                Name = name,
                UserName = userName,
                Email = email,
                Address = address,
                City = city,
                UserType = UserType.Coach,
                CoachDetail = new CoachDetail()
                {
                    Tm = registration.Tm,
                    Mf = registration.Mf,
                    Dil = registration.Dil,
                    Sozel = registration.Sozel,
                    Tyt = registration.Tyt,
                }
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
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithTags("Koç");
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

        [DefaultValue(true)]
        public required bool Tm { get; set; }
        [DefaultValue(false)]
        public required bool Mf { get; set; }
        [DefaultValue(false)]
        public required bool Sozel { get; set; }
        [DefaultValue(false)]
        public required bool Dil { get; set; }
        [DefaultValue(false)]
        public required bool Tyt { get; set; }

        public bool IsGraduated { get; set; }
    
        public byte FirstTytNet { get; set; }
    
        public bool UsedYoutube { get; set; }
    
        public bool GoneCramSchool { get; set; }

        public Guid UniversityId { get; set; }
    
        public Guid DepartmentId { get; set; }
    
        public bool Male { get; set; }
    
        //Alan değiştirdi mi
        public bool ChangedSection { get; set; }
    
        public string FromSection { get; set; }
    
        public string ToSection { get; set; }
    
        public uint Rank { get; set; }

        public byte Quota { get; set; }
        
    }
}