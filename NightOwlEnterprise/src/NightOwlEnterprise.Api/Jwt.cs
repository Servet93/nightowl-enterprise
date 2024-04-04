using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace NightOwlEnterprise.Api;

public class JwtConfig
{
    public const string JwtSection = "Jwt";
    
    public string? Issuer { get; init; }
    
    public string? Audience { get; init; }
    
    public string? Key { get; init; }
}

public class JwtHelper
{
    private readonly JwtConfig jwtConfig;
    public JwtHelper(IOptions<JwtConfig> jwtConfigOptions)
    {
        jwtConfig = jwtConfigOptions.Value;
    }
    
    public (string, DateTime) CreateRefreshToken()
    {
        var bytes = new byte[32];
        using var rnd = RandomNumberGenerator.Create();
        rnd.GetBytes(bytes);

        return (Convert.ToBase64String(bytes), DateTime.UtcNow.AddHours(12));
    }
    
    public (string, DateTime) CreateToken(ApplicationUser user)
    {
        var expiration = DateTime.UtcNow.AddHours(6);
        var keyBytes = Encoding.ASCII.GetBytes(jwtConfig.Key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            }),
            Expires = expiration,
            Issuer = jwtConfig.Issuer,
            Audience = jwtConfig.Audience,
            SigningCredentials = new SigningCredentials
            (new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var stringToken = tokenHandler.WriteToken(token);
            return (stringToken, expiration);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        // var jwtToken = tokenHandler.WriteToken(token);
        
        return (string.Empty, expiration);
    }
}