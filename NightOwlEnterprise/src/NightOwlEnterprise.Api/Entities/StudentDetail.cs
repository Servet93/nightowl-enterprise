using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Entities;

public class StudentDetail
{
    public Guid StudentId { get; set; }

    public string? Name { get; set; }
    
    public string? Surname { get; set; }
    
    public string? Email { get; set; }
    
    public string? Mobile { get; set; }
    
    public string? ParentName { get; set; }
    
    public string? ParentSurname { get; set; }
    
    public string? ParentEmail { get; set; }
    
    public string? ParentMobile { get; set; }
    
    public string? HighSchool { get; set; }
    
    public float? HighSchoolGPA { get; set; }
    
    public byte? TytGoalNet { get; set; }
    
    public byte? AytGoalNet { get; set; }
    
    public uint? GoalRanking { get; set; }

    public string? DesiredProfessionSchoolField { get; set; }
    
    public string? ExpectationsFromCoaching { get; set; }

    public bool? School { get; set; }
    
    public bool? Course { get; set; }
    
    public bool? Youtube { get; set; }
    
    public bool? PrivateTutoringTyt { get; set; }
    
    public bool? PrivateTutoringAyt { get; set; }
    
    public Grade Grade { get; set; }
    
    public ExamType ExamType { get; set; }
    
    public ApplicationUser Student { get; set; } // İlişkiyi burada tanımlıyoruz
    
    public StudentStatus Status { get; set; }
}