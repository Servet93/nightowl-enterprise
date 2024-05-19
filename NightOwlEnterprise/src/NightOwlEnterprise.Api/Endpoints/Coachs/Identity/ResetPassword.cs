using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Endpoints.Coachs.Identity;

public static class ResetPassword
{
    public static void MapResetPassword<TUser>(this IEndpointRouteBuilder endpoints)
        where TUser : class, new()
    {
        endpoints.MapPost("/resetPassword", async Task<Results<Ok, ProblemHttpResult>>
            ([FromBody] ResetPasswordRequest resetRequest, [FromServices] IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<ApplicationUserManager>();

            var user = await userManager.FindByEmailAsync(resetRequest.Email);
            
            if (user is null || user.UserType == UserType.Student)
            {
                return CommonErrorDescriptor.NotFoundEmail().CreateProblem("Password Güncelleme Başarısız!");
            }
                
            // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
            // returned a 400 for an invalid code given a valid user email.
            // return TypedResults.Problem(new ValidationProblemDetails()
            // {
            //     Detail = error.Description,
            //     Status = StatusCodes.Status400BadRequest,
            //     Errors = new Dictionary<string, string[]>()
            //     {
            //         { error.Code, new string[1] { error.Description } }
            //     },
            //     Title = "Password Reset Token"
            // });
                
            // return IdentityResult.Failed(TurkishIdentityErrorDescriber.InvalidPasswordResetToken()).CreateValidationProblem();

            IdentityResult result;
            try
            {
                if (!user.PasswordResetCodeExpiration.HasValue || user.PasswordResetCodeExpiration.Value < DateTime.UtcNow || user.PasswordResetCode != resetRequest.ResetCode)
                {
                    return CommonErrorDescriptor.InvalidPasswordResetCode().CreateProblem("Password Güncelleme Başarısız!");
                }

                result = await userManager.ResetPasswordAsync(user, resetRequest.NewPassword);
                
                //var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));
                //result = await userManager.ResetPasswordAsync(user, code, resetRequest.NewPassword);
            }
            catch (FormatException)
            {
                result = IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken());
            }

            if (!result.Succeeded)
            {
                var dict = new List<ErrorDescriptor>();
        
                foreach (var _error in result.Errors)
                {
                    dict.Add(new ErrorDescriptor(_error.Code, _error.Description));
                }

                return dict.CreateProblem("Password Güncelleme Başarısız!");
            }

            user.PasswordResetCode = null;
            user.PasswordResetCodeExpiration = null;

            await userManager.UpdateAsync(user);
            
            return TypedResults.Ok();
        }).ProducesValidationProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachIdentity);
    }
}