using System.Text.Json.Serialization;
using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Endpoints.CommonDto;

public class StudentProgramInfo
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string Text { get; set; }
        public List<StudentProgramWeekInfo> Weeklies { get; set; } = new();
    }

    public class StudentProgramWeekInfo
    {
        public Guid Id { get; set; }
    
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public string Text { get; set; }
        
        public bool IsCurrent { get; set; }
    }
    
    public class StudentProgramDayInfo
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        
        public string DateText { get; set; }
        
        public string DayText { get; set; }
        
        public int DayOfMonth { get; set; }
        
        public List<StudentProgramTaskSummarizedInfo> Tasks { get; set; } = new();
    }

    public class StudentProgramTaskSummarizedInfo
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }
    
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TaskState State { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TaskType TaskType { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ProgramExamType ExamType { get; set; }
    
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Lesson Lesson { get; set; }
        
        public string Subject { get; set; }
    }
    
    public class StudentProgramDayTaskInfo
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TaskState State { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ProgramExamType ExamType { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Lesson Lesson { get; set; }
        
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TaskType TaskType { get; set; }
        
        public string Subject { get; set; }
        
        public string Resource { get; set; }

        public ushort? QuestionCount { get; set; }
    
        public ushort? EstimatedMinute { get; set; }
        
        public ushort? CompletedMinute { get; set; }
    
        public string Not { get; set; }
        
        public string Excuese { get; set; }
    }

public class StudentProgramDailyInfo
{
    public byte TotalTask { get; set; }
    
    public byte CompletedTask { get; set; }
    
    public byte PartialCompletedTask { get; set; }
    
    public List<StudentProgramTaskSummarizedInfo> DailyTaskSummarizedInfos { get; set; }
}