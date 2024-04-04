using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace NightOwlEnterprise.Api.Endpoints.Students;

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
            
            var error = TurkishIdentityErrorDescriber.InvalidPasswordResetCode();
            
            if (user is null)
            {
                // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
                // returned a 400 for an invalid code given a valid user email.
                return TypedResults.Problem(new ValidationProblemDetails()
                {
                    Detail = error.Description,
                    Status = StatusCodes.Status400BadRequest,
                    Errors = new Dictionary<string, string[]>()
                    {
                        { error.Code, new string[1] { error.Description } }
                    },
                    Title = "Password Reset Token"
                });
                
                // return IdentityResult.Failed(TurkishIdentityErrorDescriber.InvalidPasswordResetToken()).CreateValidationProblem();
            }

            IdentityResult result;
            try
            {
                if (!user.PasswordResetCodeExpiration.HasValue || user.PasswordResetCodeExpiration.Value < DateTime.UtcNow)
                {
                    return TypedResults.Problem(new ValidationProblemDetails()
                    {
                        Detail = error.Description,
                        Status = StatusCodes.Status400BadRequest,
                        Errors = new Dictionary<string, string[]>()
                        {
                            { error.Code, new string[1] { error.Description } }
                        },
                        Title = "Password Reset Token"
                    });
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
                var dict = new Dictionary<string, string[]>();
        
                foreach (var _error in result.Errors)
                {
                    dict.Add(_error.Code, new string[1] {_error.Description});    
                }
                
                return TypedResults.Problem(new ValidationProblemDetails()
                {
                    Detail = "Password güncelleme başarısız.",
                    Status = StatusCodes.Status400BadRequest,
                    Errors = dict,
                    Title = "Password Reset Token"
                });
                
                //return result.CreateValidationProblem();
            }

            user.PasswordResetCode = null;
            user.PasswordResetCodeExpiration = null;

            await userManager.UpdateAsync(user);
            
            return TypedResults.Ok();
        }).ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }
}