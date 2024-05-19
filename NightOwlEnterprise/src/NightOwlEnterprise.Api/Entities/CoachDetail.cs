using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Entities;

public class CoachDetail
{
    public Guid CoachId { get; set; }
    
    public ApplicationUser Coach { get; set; } // İlişkiyi burada tanımlıyoruz

    public string Name { get; set; }
    public string Surname { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
    public DateTime BirthDate { get; set; }
        
    public DepartmentType DepartmentType { get; set; }
    
    public Guid UniversityId { get; set; }
    
    public string DepartmentName { get; set; }
    
    // Universite referansı
    public University University { get; set; } 
    
    public string HighSchool { get; set; }

    public float HighSchoolGPA { get; set; }
    public byte FirstTytNet { get; set; }
        
    public byte LastTytNet { get; set; }
        
    public byte FirstAytNet { get; set; }
        
    public byte LastAytNet { get; set; }
    
    public bool ChangedDepartmentType { get; set; }

    public DepartmentType FromDepartment { get; set; }
        
    public DepartmentType ToDepartment { get; set; }

    // public bool Tm { get; set; }
    // public bool Mf { get; set; }
    // public bool Sozel { get; set; }
    // public bool Dil { get; set; }
    // public bool Tyt { get; set; }

    public bool IsGraduated { get; set; }
    
    public bool UsedYoutube { get; set; }
    
    public bool GoneCramSchool { get; set; }
    
    public bool School { get; set; }
    
    public bool PrivateTutoring { get; set; }
    
    public bool Male { get; set; }
    public uint Rank { get; set; }
    public byte StudentQuota { get; set; }
    public byte MondayQuota { get; set; }
    
    public byte TuesdayQuota { get; set; }
    
    public byte WednesdayQuota { get; set; }
    
    public byte ThursdayQuota { get; set; }
    
    public byte FridayQuota { get; set; }
    
    public byte SaturdayQuota { get; set; }
    
    public byte SundayQuota { get; set; }

    public CoachStatus Status { get; set; }
}