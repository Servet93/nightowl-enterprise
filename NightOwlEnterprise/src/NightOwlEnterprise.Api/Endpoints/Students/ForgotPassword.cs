using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public static class ForgotPassword
{
    public static void MapForgotPassword<TUser>(this IEndpointRouteBuilder endpoints, IEmailSender<TUser> emailSender)
        where TUser : class, new()
    {
        // TODO: Sınırsız bir şekilde çağrılamasın. Bir sınır getirilmeli 5 dk'da bi kere gibi. 
        endpoints.MapPost("/forgotPassword", async Task<Results<Ok, ValidationProblem>>
            ([FromBody] ForgotPasswordRequest resetRequest, [FromServices] IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<TUser>>();
            var user = await userManager.FindByEmailAsync(resetRequest.Email);

            // if (user is not null && await userManager.IsEmailConfirmedAsync(user))
            if (user is not null)
            {
                var code = await userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                await emailSender.SendPasswordResetCodeAsync(user, resetRequest.Email, HtmlEncoder.Default.Encode(code));
            }

            // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
            // returned a 400 for an invalid code given a valid user email.
            return TypedResults.Ok();
        });
    }
}