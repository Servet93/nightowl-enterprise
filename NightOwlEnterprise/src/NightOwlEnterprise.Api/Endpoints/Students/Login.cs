using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public static class Login
{
    private static readonly EmailAddressAttribute _emailAddressAttribute = new();
    
    public static void MapLogin<TUser>(this IEndpointRouteBuilder endpoints, JwtHelper jwtHelper)
        where TUser : class, new()
    {
        endpoints.MapPost("/login", async Task<Results<Ok<AccessTokenResponse>, ProblemHttpResult>>
            ([FromBody] StudentLoginRequest login, [FromServices] IServiceProvider sp) =>
        {
            if (string.IsNullOrEmpty(login.Email))
            {
                return TypedResults.Problem(new ValidationProblemDetails()
                {
                    Detail = "Email boş geçilemez!",
                    Status = StatusCodes.Status401Unauthorized,
                    Errors = new Dictionary<string, string[]>()
                    {
                        { nameof(login.Email), new string[1] { "Email boş geçilemez!" } }
                    },
                    Title = "Unauthorized"
                });
                
                // return TypedResults.Problem("Email boş geçilemez!", statusCode: StatusCodes.Status401Unauthorized);
            }
            
            if (!_emailAddressAttribute.IsValid(login.Email))
            {
                return TypedResults.Problem(new ValidationProblemDetails()
                {
                    Detail = $"Email '{login.Email}' geçersiz",
                    Status = StatusCodes.Status401Unauthorized,
                    Errors = new Dictionary<string, string[]>()
                    {
                        { nameof(login.Email), new string[1] { $"Email '{login.Email}' geçersiz" } }
                    },
                    Title = "Unauthorized"
                });
                
                // return TypedResults.Problem($"Email '{login.Email}' geçersiz", statusCode: StatusCodes.Status401Unauthorized);
            }
            
            if (string.IsNullOrEmpty(login.Password))
            {
                return TypedResults.Problem(new ValidationProblemDetails()
                {
                    Detail = $"Password boş geçilemez!",
                    Status = StatusCodes.Status401Unauthorized,
                    Errors = new Dictionary<string, string[]>()
                    {
                        { nameof(login.Password), new string[1] { $"Password boş geçilemez!" } }
                    },
                    Title = "Unauthorized"
                });
                
                //return TypedResults.Problem("Password boş geçilemez!", statusCode: StatusCodes.Status401Unauthorized);
            }
            
            var signInManager = sp.GetRequiredService<SignInManager<ApplicationUser>>();
            
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            
            signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
            
            var user = await userManager.FindByEmailAsync(login.Email);

            if (user is null)
            {
                return TypedResults.Problem(new ValidationProblemDetails()
                {
                    Detail = $"Email veya password hatalı!",
                    Status = StatusCodes.Status401Unauthorized,
                    Errors = new Dictionary<string, string[]>()
                    {
                        { nameof(login.Email), new string[1] { $"Email hatalı olabilir" } },
                        { nameof(login.Password), new string[1] { $"Password hatalı olabilir" } }
                    },
                    Title = "Unauthorized"
                });
                
                //return TypedResults.Problem("Email veya password hatalı!", statusCode: StatusCodes.Status401Unauthorized);
            }

            //PasswordSignIn uses username
            // var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent: false, lockoutOnFailure: true);
            var result = await signInManager.CheckPasswordSignInAsync(user, login.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                return TypedResults.Problem(new ValidationProblemDetails()
                {
                    Detail = $"Email veya password hatalı!",
                    Status = StatusCodes.Status401Unauthorized,
                    Errors = new Dictionary<string, string[]>()
                    {
                        { nameof(login.Email), new string[1] { $"Email hatalı olabilir" } },
                        { nameof(login.Password), new string[1] { $"Password hatalı olabilir" } }
                    },
                    Title = "Unauthorized"
                });
                //return TypedResults.Problem("Email veya password hatalı!", statusCode: StatusCodes.Status401Unauthorized);
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
        }).ProducesValidationProblem(StatusCodes.Status401Unauthorized);
    }
    
    public sealed class StudentLoginRequest
    {
        [DefaultValue("nightowl-enterprise@gmail.com")]
        public required string Email { get; init; }

        [DefaultValue("Aa.123456")]
        public required string Password { get; init; }
    }
}