using NightOwlEnterprise.Api.Endpoints;
using NightOwlEnterprise.Api.Entities.Enums;
using NightOwlEnterprise.Api.Entities.Nets;
using NightOwlEnterprise.Api.Entities.PrivateTutoring;

namespace NightOwlEnterprise.Api.Entities;

public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser<Guid>
{
    public string Name { get; set; } = String.Empty;
    public string Address { get; set; }  = String.Empty;
    public string City { get; set; }  = String.Empty;
    
    public UserType UserType { get; set; }
    
    public string CustomerId { get; set; } = String.Empty;
    
    public string SubscriptionId { get; set; } = String.Empty;
    
    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiration { get; set; }
    
    public string? PasswordResetCode { get; set; }
    
    public DateTime? PasswordResetCodeExpiration { get; set; }
    
    public CoachDetail CoachDetail { get; set; }
    
    public StudentDetail StudentDetail { get; set; }
    
    public ProfilePhoto ProfilePhoto { get; set; }
    public ICollection<SubscriptionHistory> SubscriptionHistories { get; set; } = new List<SubscriptionHistory>();
    
    public ICollection<Invitation> InvitationsAsCoach { get; set; } = new List<Invitation>();
    
    public ICollection<Invitation> InvitationsAsStudent { get; set; } = new List<Invitation>();

    public ICollection<CoachYksRanking> CoachYksRankings { get; set; } = new List<CoachYksRanking>();

    public PrivateTutoringTYT PrivateTutoringTYT { get; set; }
    public PrivateTutoringMF PrivateTutoringMF { get; set; }
    public PrivateTutoringTM PrivateTutoringTM { get; set; }
    public PrivateTutoringSozel PrivateTutoringSozel { get; set; }
    public PrivateTutoringDil PrivateTutoringDil { get; set; }
    
    public TYTNets TytNets { get; set; }
    public TMNets TmNets { get; set; }
    public MFNets MfNets { get; set; }
    public SozelNets SozelNets { get; set; }
    public DilNets DilNets { get; set; }
    public ICollection<CoachStudentTrainingSchedule> CoachStudentTrainingSchedules { get; set; } = new List<CoachStudentTrainingSchedule>();

    public ICollection<CoachStudentTrainingSchedule> StudentCoachTrainingSchedules { get; set; } = new List<CoachStudentTrainingSchedule>();
    
    public ResourcesTYT ResourcesTYT { get; set; }
    
    public ResourcesAYT ResourcesAYT { get; set; }
}