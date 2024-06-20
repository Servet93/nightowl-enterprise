namespace NightOwlEnterprise.Api.Entities;

public class VoiceCallsHistory
{
    public Guid Id { get; set; }
    
    public Guid? InvitationId { get; set; }
    
    public string? Source { get; set; }
    
    public string? Destination { get; set; }
    
    public string? Pair { get; set; }
    
    public string? Content { get; set; }
    
    public string? Status { get; set; }
    
    public string? CallDetailResult { get; set; }
    
    public string? SourceResult { get; set; }
    
    public string? DestinationResult { get; set; }
    
    public bool? Ok { get; set; }
    
    public DateTime? CreatedAt { get; set; }
}