using System.Text.Json.Serialization;
using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Endpoints.CommonDto;

public class CallInfoResponse
{
    public VideoCall VideoCall { get; set; }
        
    public VoiceCall VoiceCall { get; set; }
}

public class VideoCall
{
    public Guid Id { get; set; }

    public DateTime Date { get; set; }

    public TimeSpan StartTime { get; set; }
        
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public InvitationState State { get; set; }

    public bool Enabled { get; set; }
        
    public string JoinUrl { get; set; }
}
    
public class VoiceCall
{
    public Guid Id { get; set; }

    public DateTime Date { get; set; }

    public TimeSpan StartTime { get; set; }
        
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public  InvitationState State { get; set; }

    public bool Enabled { get; set; }
}