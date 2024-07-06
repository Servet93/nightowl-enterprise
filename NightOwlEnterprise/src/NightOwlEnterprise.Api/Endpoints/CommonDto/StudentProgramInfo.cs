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
        public List<StudentProgramWeeklyInfo> Weeklies { get; set; } = new();
    }

    public class StudentProgramWeeklyInfo
    {
        public Guid Id { get; set; }
    
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public string Text { get; set; }
        
        public bool IsCurrent { get; set; }
    }
    
    public class StudentProgramDailyInfo
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        
        public string DateText { get; set; }
        
        public string DayText { get; set; }
        
        public List<StudentProgramDailyTaskSummarizedInfo> Tasks { get; set; } = new();
    }

    public class StudentProgramDailyTaskSummarizedInfo
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
    
    public class StudentProgramDailyTaskInfo
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