using System.Text.Json.Serialization;
using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Endpoints.Common;

public class AccessTokenResponse
{
    public string AccessToken { get; set; }
    public DateTime AccessTokenExpiration { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserType UserType { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CoachStatus? CoachStatus { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StudentStatus? StudentStatus { get; set; }
}