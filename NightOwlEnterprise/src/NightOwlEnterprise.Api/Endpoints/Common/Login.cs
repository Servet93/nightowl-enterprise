using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NightOwlEnterprise.Api.Entities;
using Swashbuckle.AspNetCore.Filters;

namespace NightOwlEnterprise.Api.Endpoints.Common;

public static class Login
{
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();
    
    public static void MapLogin(this IEndpointRouteBuilder endpoints, JwtHelper jwtHelper)
    {
        endpoints.MapPost("/login", async Task<Results<Ok<AccessTokenResponse>, ProblemHttpResult>>
            ([FromBody] LoginRequest login, [FromServices] IServiceProvider sp) =>
        {
            var identityErrors = new List<IdentityError>();
            
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            
            var errorDescriber = sp.GetRequiredService<TurkishIdentityErrorDescriber>();
            
            if (string.IsNullOrEmpty(login.Email) || !_emailAddressAttribute.IsValid(login.Email))
            {
                identityErrors.Add(errorDescriber.InvalidEmail(login.Email));
            }
            
            if (string.IsNullOrEmpty(login.Password))
            {
                identityErrors.Add(errorDescriber.EmptyPassword());
            }
            
            if (identityErrors.Any())
            {
                return identityErrors.CreateProblem("Oturum açma işlemi başarısız!");
            }
            
            var signInManager = sp.GetRequiredService<SignInManager<ApplicationUser>>();
            
            signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
            
            var user = await userManager.FindByEmailAsync(login.Email);

            if (user is null)
            {
                return errorDescriber.WrongEmailOrPassword().CreateProblem("Oturum açma işlemi başarısız!");
            }

            //PasswordSignIn uses username
            // var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent: false, lockoutOnFailure: true);
            var result = await signInManager.CheckPasswordSignInAsync(user, login.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                return errorDescriber.WrongEmailOrPassword().CreateProblem("Oturum açma işlemi başarısız!");
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
        }).ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }
    
    public sealed class LoginRequest
    {
        [DefaultValue("nightowl-enterprise@gmail.com")]
        public required string Email { get; init; }

        [DefaultValue("Aa.123456")]
        public required string Password { get; init; }
    }
    
    public class LoginRequestExamples : IMultipleExamplesProvider<LoginRequest>
    {
        public IEnumerable<SwaggerExample<LoginRequest>> GetExamples()
        {
            yield return SwaggerExample.Create("Öğrenci -> Servet,Package:Coach", new LoginRequest()
            {
                Email = "servet-package-coach@gmail.com",
                Password = "Aa123456",
            });
            
            yield return SwaggerExample.Create("Öğrenci -> Servet,Package:Pdr", new LoginRequest()
            {
                Email = "servet-package-pdr@gmail.com",
                Password = "Aa123456",
            });
            
            yield return SwaggerExample.Create("Öğrenci -> Burak,Package:Coach", new LoginRequest()
            {
                Email = "burak-package-coach@gmail.com",
                Password = "Aa123456",
            });
            
            yield return SwaggerExample.Create("Öğrenci -> Burak,Package:Pdr", new LoginRequest()
            {
                Email = "burak-package-pdr@gmail.com",
                Password = "Aa123456",
            });
            
            yield return SwaggerExample.Create("Öğrenci -> Eren,Package:Coach", new LoginRequest()
            {
                Email = "eren-package-coach@gmail.com",
                Password = "Aa123456",
            });
            
            yield return SwaggerExample.Create("Öğrenci -> Eren,Package:Pdr", new LoginRequest()
            {
                Email = "eren-package-pdr@gmail.com",
                Password = "Aa123456", 
            });
            
            yield return SwaggerExample.Create("Öğrenci -> Turgay,Package:Coach", new LoginRequest()
            {
                Email = "turgay-package-coach@gmail.com",
                Password = "Aa123456", 
            });
            
            yield return SwaggerExample.Create("Öğrenci -> Turgay,Package:Pdr", new LoginRequest()
            {
                Email = "turgay-package-pdr@gmail.com",
                Password = "Aa123456",
            });
            
            yield return SwaggerExample.Create("Koç -> Servet,CoachType:Coach", new LoginRequest()
            {
                Email = "servet-coach@gmail.com",
                Password = "Aa123456",
            });
            
            yield return SwaggerExample.Create("Pdr -> Servet,CoachType:Pdr", new LoginRequest()
            {
                Email = "servet-pdr@gmail.com",
                Password = "Aa123456",
            });
            
            yield return SwaggerExample.Create("Koç -> Burak,Package:Coach", new LoginRequest()
            {
                Email = "burak-coach@gmail.com",
                Password = "Aa123456",
            });
            
            yield return SwaggerExample.Create("Pdr -> Burak,Package:Pdr", new LoginRequest()
            {
                Email = "burak-pdr@gmail.com",
                Password = "Aa123456",
            });
            yield return SwaggerExample.Create("Koç -> Eren,Package:Coach", new LoginRequest()
            {
                Email = "eren-koc@gmail.com",
                Password = "Aa123456",
            });
            
            yield return SwaggerExample.Create("Pdr -> Eren,Package:Pdr", new LoginRequest()
            {
                Email = "eren-pdr@gmail.com",
                Password = "Aa123456",
            });
            yield return SwaggerExample.Create("Koç -> Turgay,Package:Coach", new LoginRequest()
            {
                Email = "turgay-koc@gmail.com",
                Password = "Aa123456",
            });
            yield return SwaggerExample.Create("Pdr -> Turgay,Package:Pdr", new LoginRequest()
            {
                Email = "turgay-pdr@gmail.com",
                Password = "Aa123456",
            });
            yield return SwaggerExample.Create("Koç -> Alper Demir,CoachType:Coach", new LoginRequest()
            {
                Email = "alper.demir@example.com",
                Password = "Aa123456",
            });
        }
    }
}