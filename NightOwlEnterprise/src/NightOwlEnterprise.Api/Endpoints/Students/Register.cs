using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Swashbuckle.AspNetCore.Annotations;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public static class Register
{
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();
    
    public static void MapRegister<TUser>(this IEndpointRouteBuilder endpoints, IEmailSender<StudentApplicationUser> emailSender, LinkGenerator linkGenerator)
        where TUser : class, new()
    {
        // NOTE: We cannot inject UserManager<TUser> directly because the TUser generic parameter is currently unsupported by RDG.
        // https://github.com/dotnet/aspnetcore/issues/47338
        endpoints.MapPost("/register", async Task<Results<Ok, ValidationProblem>>
            ([FromBody] StudentRegisterRequest registration, HttpContext context, [FromServices] IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<StudentApplicationUser>>();
            var errorDescriber = sp.GetRequiredService<TurkishIdentityErrorDescriber>();

            if (!userManager.SupportsUserEmail)
            {
                throw new NotSupportedException($"{nameof(MapRegister)} requires a user store with email support.");
            }
            
            var name = registration.Name;
            var surname = registration.Surname;
            var email = registration.Email;
            var phoneNumber = registration.PhoneNumber;
            
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
            
            if (string.IsNullOrEmpty(surname) || surname.Length < 2)
            {
                return IdentityResult.Failed(errorDescriber.InvalidSurname(surname)).CreateValidationProblem();
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
                return IdentityResult.Failed(errorDescriber.InvalidMobile(phoneNumber)).CreateValidationProblem();
            }

            var user = new StudentApplicationUser()
            {
                Name = name,
                Surname = surname,
                UserName = email,
                Email = email,
                PhoneNumber = phoneNumber,
            };
            
            var result = await userManager.CreateAsync(user, registration.Password);

            if (!result.Succeeded)
            {
                return result.CreateValidationProblem();
            }

            await SendConfirmationEmailAsync(user, userManager, context, email);
            return TypedResults.Ok();
        });
        
        async Task SendConfirmationEmailAsync(StudentApplicationUser user, UserManager<StudentApplicationUser> userManager, HttpContext context, string email, bool isChange = false)
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
    
    private sealed class StudentRegisterRequest
    {
        [DefaultValue("NightOwl")]
        public required string Name { get; init; }
        
        [DefaultValue("Enterprise")]
        public required string Surname { get; init; }
        
        [DefaultValue("nightowl-enterprise@gmail.com")]
        public required string Email { get; init; }

        [DefaultValue("Aa.123456")]
        public required string Password { get; init; }
        
        [SwaggerSchema("The student's phone number", Description = "phone number format is 5xx-xxx-xxx-xxx")]
        [DefaultValue("533-333-33-33")]
        public required string PhoneNumber { get; init; }
        
        public required Dictionary<string,string> QuestionIdsToAnswers { get; init; }
    } 
}