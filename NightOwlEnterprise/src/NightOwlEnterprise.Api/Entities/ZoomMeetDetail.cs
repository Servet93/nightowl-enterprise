namespace NightOwlEnterprise.Api.Entities;

public class ZoomMeetDetail
{
    public Guid Id { get; set; }
    
    public string? MeetId { get; set; }
    
    public string? HostEmail { get; set; }
    
    public string? RegistrationUrl { get; set; }
    
    public string? JoinUrl { get; set; }
    
    public string? MeetingPasscode { get; set; }
    
    public DateTime? StartTime { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    
    public string? CoachRegistrantId { get; set; }
    
    public string? CoachParticipantPinCode { get; set; }
    
    public string? CoachJoinUrl { get; set; }
    
    public string? StudentRegistrantId { get; set; }
    
    public string? StudentParticipantPinCode { get; set; }
    
    public string? StudentJoinUrl { get; set; }
    
    public Guid InvitationId { get; set; }
    public Invitation Invitation { get; set; }
}