using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Entities;

public class Invitation
{
    public Guid Id { get; set; }
    public Guid CoachId { get; set; }
    public ApplicationUser Coach { get; set; }
    public Guid StudentId { get; set; }
    public ApplicationUser Student { get; set; }
    public DateTime Date { get; set; }
    
    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    
    public TimeSpan EndTime { get; set; }

    public InvitationType Type { get; set; }
    
    public InvitationState State { get; set; }

    public string? Excuse { get; set; }
    
    public bool IsAvailable { get; set; } // true ise görüşme aktif, false ise zamanı gelmedi // 5 dakika kala burası açılır

    public Guid? ZoomMeetDetailId { get; set; }
    
    public ZoomMeetDetail ZoomMeetDetail { get; set; }
}