using System.ComponentModel;
using System.Security.Cryptography;
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
    public static void MapForgotPassword<TUser>(this IEndpointRouteBuilder endpoints, IEmailSender<ApplicationUser> emailSender)
        where TUser : class, new()
    {
        // TODO: Sınırsız bir şekilde çağrılamasın. Bir sınır getirilmeli 5 dk'da bi kere gibi. 
        endpoints.MapPost("/forgotPassword", async Task<Results<Ok, ValidationProblem>>
            ([FromBody] ForgotPasswordRequest resetRequest, [FromServices] IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(resetRequest.Email);

            // if (user is not null && await userManager.IsEmailConfirmedAsync(user))
            if (user is not null)
            {
                if (user.PasswordResetCodeExpiration.HasValue &&
                    user.PasswordResetCodeExpiration.Value > DateTime.UtcNow)
                {
                    return TypedResults.Ok();
                }
                
                var passwordResetToken = PasswordGenerator.GeneratePassword(6);
                
                user.PasswordResetCode = passwordResetToken; 
                user.PasswordResetCodeExpiration = DateTime.UtcNow.AddMinutes(30);

                var updateResult = await userManager.UpdateAsync(user);

                await emailSender.SendPasswordResetCodeAsync(user, resetRequest.Email, passwordResetToken);
            }

            // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
            // returned a 400 for an invalid code given a valid user email.
            return TypedResults.Ok();
        });
    }
    
    public class PasswordGenerator
    {
        private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string NumericChars = "0123456789";
        // private const string SpecialChars = "!@#$%^&*()-_=+";

        public static string GeneratePassword(int length = 8)
        {
            // Rastgele sayı üreticisi
            using (var rng = new RNGCryptoServiceProvider())
            {
                // Şifre karakterleri
                var chars = LowercaseChars + UppercaseChars + NumericChars;

                // Şifre karakterlerini karıştır
                var shuffledChars = new string(chars.OrderBy(x => Guid.NewGuid()).ToArray());

                // Rastgele şifre oluştur
                var password = new char[length];
                var randomBytes = new byte[length];
                rng.GetBytes(randomBytes);
                for (int i = 0; i < length; i++)
                {
                    password[i] = shuffledChars[randomBytes[i] % shuffledChars.Length];
                }

                // Şifre uzunluğunu kontrol et
                if (password.Distinct().Count() < 4)
                {
                    // Eğer şifrede en az 4 farklı karakter yoksa, yeniden oluştur
                    return GeneratePassword(length);
                }

                // Şifreyi döndür
                return new string(password);
            }
        }
    }
}