using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NightOwlEnterprise.Api.Migrations;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public static class Login
{
    public static void MapLogin<TUser>(this IEndpointRouteBuilder endpoints, JwtHelper jwtHelper)
        where TUser : class, new()
    {
        endpoints.MapPost("/login", async Task<Results<Ok<AccessTokenResponse>, ProblemHttpResult>>
            ([FromBody] StudentLoginRequest login, [FromServices] IServiceProvider sp) =>
        {
            var signInManager = sp.GetRequiredService<SignInManager<ApplicationUser>>();
            
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            
            signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;

            var user = await userManager.FindByEmailAsync(login.Email);

            if (user is null)
            {
                return TypedResults.Problem("Failed", statusCode: StatusCodes.Status401Unauthorized);
            }

            //PasswordSignIn uses username
            // var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent: false, lockoutOnFailure: true);
            var result = await signInManager.CheckPasswordSignInAsync(user, login.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
            }

            var tokenResult = jwtHelper.CreateToken(user);
            var refreshTokenResult = jwtHelper.CreateRefreshToken();

            user.RefreshToken = refreshTokenResult.Item1;
            user.RefreshTokenExpiration = refreshTokenResult.Item2;

            var updateResult = await userManager.UpdateAsync(user);
            
            
            // The signInManager already produced the needed response in the form of a cookie or bearer token.
            return TypedResults.Ok<AccessTokenResponse>(new AccessTokenResponse()
            {
                AccessToken = tokenResult.Item1,
                AccessTokenExpiration = tokenResult.Item2,
                RefreshToken = refreshTokenResult.Item1,
                RefreshTokenExpiration = refreshTokenResult.Item2
            });
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