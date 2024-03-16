using System.ComponentModel;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public static class Login
{
    public static void MapLogin<TUser>(this IEndpointRouteBuilder endpoints)
        where TUser : class, new()
    {
        endpoints.MapPost("/login", async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>>
            ([FromBody] StudentLoginRequest login, [FromServices] IServiceProvider sp) =>
        {
            var signInManager = sp.GetRequiredService<SignInManager<TUser>>();
            
            var userManager = sp.GetRequiredService<UserManager<TUser>>();
            
            signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;

            var user = await userManager.FindByEmailAsync(login.Email);

            if (user is null)
            {
                return TypedResults.Problem("Failed", statusCode: StatusCodes.Status401Unauthorized);
            }

            //PasswordSignIn uses username
            // var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent: false, lockoutOnFailure: true);
            var result = await signInManager.PasswordSignInAsync(user, login.Password, isPersistent: false, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
            }

            // The signInManager already produced the needed response in the form of a cookie or bearer token.
            return TypedResults.Empty;
        });
    }
    
    public sealed class StudentLoginRequest
    {
        [DefaultValue("nightowl-enterprise@gmail.com")]
        public required string Email { get; init; }

        [DefaultValue("Aa.123456")]
        public required string Password { get; init; }
    }
}