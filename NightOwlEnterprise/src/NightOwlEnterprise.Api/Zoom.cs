using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NightOwlEnterprise.Api;

public class ZoomCredential
{
    public const string ZoomSection = "Zoom";

    //For Backend
    public string? ClientId { get; set; }
    
    public string? ClientSecret { get; set; }
    
    public string? AccountId { get; set; }
    
    public string? GrantType { get; set; }
    
    public string? ZoomOAuthUrl { get; set; }

    // public const string ZoomApiUrl = "https://api.zoom.us/v2/users/me";
    
    public string? ZoomApiBaseUrl { get; set; }
}

public class ZoomTokenResponse
{
    [JsonProperty("access_token")]
    public string? AccessToken { get; set; }
    
    [JsonProperty("token_type")]
    public string? TokenType { get; set; }
    
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonProperty("scope")]
    public string? Scope { get; set; }
}

public class ZoomMeetCreationResponse
{
    [JsonProperty("id")]
    public string? Id { get; set; }
    
    [JsonProperty("host_email")]
    public string? HostEmail { get; set; }
    
    [JsonProperty("registration_url")]
    public string? RegistrationUrl { get; set; }
    
    [JsonProperty("join_url")]
    public string? JoinUrl { get; set; }
    
    [JsonProperty("password")]
    public string? MeetingPasscode { get; set; }
    
    [JsonProperty("start_time")]
    public DateTime? StartTime { get; set; }
    
    [JsonProperty("created_at")]
    public DateTime? CreatedAt { get; set; }
}

public class ZoomAddRegistrantResponse
{
    [JsonProperty("id")]
    public string? Id { get; set; }
    
    [JsonProperty("join_url")]
    public string? JoinUrl { get; set; }
    
    [JsonProperty("registrant_id")]
    public string? RegistrantId { get; set; }
    
    [JsonProperty("start_time")]
    public DateTime? StartTime { get; set; }
    
    [JsonProperty("participant_pin_code")]
    public string? ParticipantPinCode { get; set; }
}


public class Zoom
{
    private string? AccessToken { get; set; }
    
    private DateTime? AccessTokenExpiration { get; set; }

    public ZoomCredential _zoomCredential { get; set; }

    public Zoom(ZoomCredential zoomCredential)
    {
        _zoomCredential = zoomCredential;
    }
    
    private async Task GetZoomAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(AccessToken) && AccessTokenExpiration > DateTime.UtcNow.AddMinutes(5))
        {
            return;
        }
        
        using var httpClient = new HttpClient();
        
        var requestData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", _zoomCredential.GrantType!),
            new KeyValuePair<string, string>("account_id", _zoomCredential.AccountId!),
        });
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, _zoomCredential.ZoomOAuthUrl)
        {
            Content = requestData
        };

        var basicValueAsBytes = System.Text.Encoding.UTF8.GetBytes($"{_zoomCredential.ClientId}:{_zoomCredential.ClientSecret}");
        
        var basicValueAsBase64 = Convert.ToBase64String(basicValueAsBytes);
        
        requestMessage.Headers.Add("Authorization", $"Basic {basicValueAsBase64}");
        
        var response = await httpClient.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();
        
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using StreamReader streamReader = new StreamReader(responseStream);
        using JsonTextReader jsonReader = new JsonTextReader(streamReader);
        var serializer = new JsonSerializer();
        var zoomTokenResponse = serializer.Deserialize<ZoomTokenResponse>(jsonReader);

        AccessToken = zoomTokenResponse.AccessToken;
        AccessTokenExpiration = DateTime.UtcNow.AddHours(1);
    }
    
    public async Task<string> GetUserIdAsync(string email = "me")
    {
        await GetZoomAccessTokenAsync();
        
        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        var response = await httpClient.GetAsync($"{_zoomCredential.ZoomApiBaseUrl}/users/{email}");

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
            
        var jsonResponse = JObject.Parse(responseContent);
            
        // Toplantı kimliğini alın
        var userId = jsonResponse["id"]!.ToString();

        return userId;
    }
    
    public async Task<ZoomMeetCreationResponse> CreateMeetingAsync(string userId, string topic, DateTime startTime, int duration)
    {
        await GetZoomAccessTokenAsync();
        
        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        var requestData = new
        {
            topic = topic,
            type = 2, // Planlanmış toplantı türü
            start_time = startTime.ToString("yyyy-MM-ddTHH:mm:ss"), // ISO 8601 formatında
            duration = duration, // Toplantı süresi dakikalarda
            timezone = "Europe/Istanbul", //UTC,
            default_password = true,
            settings = new
            {
                approval_type = 0,
                registration_type = 2,
                join_before_host = true,
                waiting_room = false,
            }
        };

        var jsonData = JsonConvert.SerializeObject(requestData);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"{_zoomCredential.ZoomApiBaseUrl}/users/{userId}/meetings", content);

        response.EnsureSuccessStatusCode();

        // Yanıtı okuyun ve JSON'a çevirin
        // var responseContent = await response.Content.ReadAsStringAsync();
        // var jsonResponse = JObject.Parse(responseContent);
        
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using StreamReader streamReader = new StreamReader(responseStream);
        using JsonTextReader jsonReader = new JsonTextReader(streamReader);
        var serializer = new JsonSerializer();
        var zoomMeetCreationResponse = serializer.Deserialize<ZoomMeetCreationResponse>(jsonReader);

        // Toplantı kimliğini alın
        // var meetingId = jsonResponse["id"].ToString();

        // Console.WriteLine($"Toplantı başarıyla oluşturuldu. Toplantı kimliği: {meetingId}");

        return zoomMeetCreationResponse;
    }

    public async Task<ZoomAddRegistrantResponse> AddRegistrantAsync(string meetingId, string registrantEmail, string name, string surname)
    {
        await GetZoomAccessTokenAsync();
        
        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        var requestData = new
        {
            email = registrantEmail,
            first_name = name,
            last_name = surname,
        };

        var jsonData = JsonConvert.SerializeObject(requestData);
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"{_zoomCredential.ZoomApiBaseUrl}/meetings/{meetingId}/registrants", content);

        response.EnsureSuccessStatusCode();
        
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using StreamReader streamReader = new StreamReader(responseStream);
        using JsonTextReader jsonReader = new JsonTextReader(streamReader);
        var serializer = new JsonSerializer();
        var zoomAddRegistrantResponse = serializer.Deserialize<ZoomAddRegistrantResponse>(jsonReader);

        return zoomAddRegistrantResponse;
    }
}