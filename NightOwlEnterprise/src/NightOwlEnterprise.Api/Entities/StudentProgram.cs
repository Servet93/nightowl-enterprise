using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Entities;

public class StudentProgram
{
    public Guid Id { get; set; }
    
    public Guid StudentId { get; set; }
    
    public Guid CoachId { get; set; }
    
    public ApplicationUser Student { get; set; }
    
    public ApplicationUser Coach { get; set; }

    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public string StartDateText { get; set; }
    
    public string EndDateText { get; set; }
    
    public List<StudentProgramWeekly> Weeklies { get; set; } = new();
}

public class StudentProgramWeekly
{
    public Guid Id { get; set; }
    
    public Guid StudentProgramId { get; set; }
    
    public StudentProgram StudentProgram { get; set; }
    
    public Guid StudentId { get; set; }
    
    public Guid CoachId { get; set; }
    
    public ApplicationUser Student { get; set; }
    
    public ApplicationUser Coach { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public string StartDateText { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public string EndDateText { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public List<StudentProgramDaily> Dailies { get; set; } = new();
}

public class StudentProgramDaily
{
    public Guid Id { get; set; }
    
    public Guid StudentProgramWeeklyId { get; set; }
    
    public StudentProgramWeekly StudentProgramWeekly { get; set; }
    
    public Guid StudentId { get; set; }
    
    public ApplicationUser Student { get; set; }
    
    public Guid CoachId { get; set; }
    
    public ApplicationUser Coach { get; set; }
    public DateTime Date { get; set; }
    
    public string DateText { get; set; }
    
    public DayOfWeek Day { get; set; }
    
    public string DayText { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public List<StudentProgramDailyTasks> DailyTasks { get; set; } = new();
}

public class StudentProgramDailyTasks
{
    public Guid Id { get; set; }

    public Guid StudentProgramDailyId { get; set; }
    
    public StudentProgramDaily StudentProgramDaily { get; set; }
    
    public TaskState State { get; set; }

    public ProgramExamType ExamType { get; set; }
    
    public TaskType TaskType { get; set; }
    
    public Lesson Lesson { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Subject { get; set; }
    
    public string? Resource { get; set; }

    public ushort? QuestionCount { get; set; }
    
    public byte? Minute { get; set; }
    
    public string? Not { get; set; }
    
    public string? Excuse { get; set; }
}





