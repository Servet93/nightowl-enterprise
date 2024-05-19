namespace NightOwlEnterprise.Api.Entities;

public class CoachStudentTrainingSchedule
{
    public Guid Id { get; set; }
    public Guid CoachId { get; set; }
    public ApplicationUser Coach { get; set; }
    public Guid StudentId { get; set; }
    public ApplicationUser Student { get; set; }
    public DayOfWeek Day { get; set; }
    
    public DateTime CreatedAt { get; set; }
}