using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NightOwlEnterprise.Api.Entities;

namespace NightOwlEnterprise.Api.Endpoints.Common;

public static class Refresh
{
    public static void MapRefresh(this IEndpointRouteBuilder endpoints, JwtHelper jwtHelper)
    {
        var timeProvider = endpoints.ServiceProvider.GetRequiredService<TimeProvider>();
        var bearerTokenOptions = endpoints.ServiceProvider.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>();
        
        endpoints.MapPost("/refresh", async Task<Results<Ok<AccessTokenResponse>, ProblemHttpResult>>
            ([FromBody] RefreshRequest refreshRequest, [FromServices] IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            var user = userManager.Users.Where(x => x.RefreshToken == refreshRequest.RefreshToken).FirstOrDefault();
            
            if (user is null || user.RefreshTokenExpiration < DateTime.Now)
            {
                return TypedResults.Problem(new ValidationProblemDetails()
                {
                    Detail = "Token not updated",
                    Status = StatusCodes.Status403Forbidden
                });
                //return TypedResults.Challenge();
            }
            
            // var refreshTokenProtector = bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
            // var refreshTicket = refreshTokenProtector.Unprotect(refreshRequest.RefreshToken);

            // Reject the /refresh attempt with a 401 if the token expired or the security stamp validation fails
            // if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc ||
            //     timeProvider.GetUtcNow() >= expiresUtc ||
            //     await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not TUser user)
            //
            // {
            //     return TypedResults.Challenge();
            // }

            var tokenResult = jwtHelper.CreateToken(user);
            var refreshTokenResult = jwtHelper.CreateRefreshToken();
            
            user.RefreshToken = refreshTokenResult.Item1;
            user.RefreshTokenExpiration = refreshTokenResult.Item2;

            var updateResult = await userManager.UpdateAsync(user);

            // var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);
            // return TypedResults.SignIn(newPrincipal, authenticationScheme: IdentityConstants.BearerScheme);
            
            return TypedResults.Ok(new AccessTokenResponse()
            {
                AccessToken = tokenResult.Item1,
                AccessTokenExpiration = tokenResult.Item2,
                RefreshToken = refreshTokenResult.Item1,
                RefreshTokenExpiration = refreshTokenResult.Item2
            });
        }).ProducesValidationProblem(401).ProducesValidationProblem(403);
    }
}