using System.Globalization;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NightOwlEnterprise.Api.Entities;
using NightOwlEnterprise.Api.Entities.Enums;
using NightOwlEnterprise.Api.Utils;

namespace NightOwlEnterprise.Api.Endpoints.Coachs.Me.Students;

public static class Program
{
    public static void MapProgram(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("me/students/{studentId}/programs",
                async Task<Results<Ok<List<StudentProgramInfo>>, ProblemHttpResult>>
                    ([FromQuery] Guid studentId, ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                    var coachId = claimsPrincipal.GetId();

                    var mountIndex = 1;
                    var weekIndex = 1;

                    var studentPrograms = dbContext.StudentPrograms
                        .Where(x => x.CoachId == coachId && x.StudentId == studentId)
                        .Include(x => x.Weeklies)
                        .OrderBy(x => x.StartDate).ToList();

                    var studentProgramInfoList = new List<StudentProgramInfo>();

                    var cultureInfo = new CultureInfo("tr-TR");

                    foreach (var studentProgram in studentPrograms)
                    {
                        var startDateText = studentProgram.StartDate.ToString("dd MMMM", cultureInfo);
                        var endDateText = studentProgram.EndDate.ToString("dd MMMM / yyyy", cultureInfo);

                        var studentProgramInfo = new StudentProgramInfo()
                        {
                            Id = studentProgram.Id,
                            StartDate = studentProgram.StartDate,
                            EndDate = studentProgram.EndDate,
                            Text =
                                $"{(mountIndex).ToString()}.Ay ({startDateText} - {endDateText})",
                        };

                        foreach (var weeklyInfo in studentProgram.Weeklies)
                        {
                            var weekStartDateText = weeklyInfo.StartDate.ToString("dd MMMM", cultureInfo);
                            var weekEndDateText = weeklyInfo.EndDate.ToString("dd MMMM / yyyy", cultureInfo);

                            studentProgramInfo.Weeklies.Add(new StudentProgramWeeklyInfo()
                            {
                                Id = weeklyInfo.Id,
                                StartDate = weeklyInfo.StartDate,
                                EndDate = weeklyInfo.EndDate,
                                Text = $"{(weekIndex).ToString()}.Hafta ({weekStartDateText} - {weekEndDateText})",
                            });

                            weekIndex++;
                        }

                        mountIndex++;
                        weekIndex = 1;

                        studentProgramInfoList.Add(studentProgramInfo);
                    }

                    return TypedResults.Ok(studentProgramInfoList);

                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachMeStudentsProgram)
            .RequireAuthorization("CoachOrPdr");

        endpoints.MapGet("me/students/{studentId}/programs-week/{weekId}",
                async Task<Results<Ok<List<StudentProgramDailyInfo>>, ProblemHttpResult>>
                ([FromQuery] Guid studentId, [FromQuery] Guid weekId, ClaimsPrincipal claimsPrincipal,
                    [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                    var coachId = claimsPrincipal.GetId();

                    var studentProgramDailyInfoList = dbContext.StudentProgramDaily
                        .Where(x => x.StudentProgramWeeklyId == weekId && x.StudentId == studentId &&
                                    x.CoachId == coachId)
                        .Include(x => x.DailyTasks)
                        .OrderBy(x => x.Date)
                        .Select(x => new StudentProgramDailyInfo()
                        {
                            Id = x.Id,
                            Date = x.Date,
                            DateText = x.DateText,
                            DayText = x.DayText,
                            Tasks = x.DailyTasks.Select(y => new StudentProgramDailyTaskSummarizedInfo()
                            {
                                Id = y.Id,
                                Lesson = y.Lesson,
                                State = y.State,
                                Subject = y.Subject,
                                ExamType = y.ExamType
                            }).ToList()
                        })
                        .ToList();
                    
                    studentProgramDailyInfoList.ForEach(x =>
                    {
                        x.Tasks.ForEach(y =>
                        {
                            y.Title =
                                $"{ProgramExamTypeUtil.GetName(y.ExamType)} - {LessonUtil.GetName(y.Lesson)} - {TaskTypeUtil.GetName(y.TaskType)}";
                        });
                    });

                    return TypedResults.Ok(studentProgramDailyInfoList);

                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachMeStudentsProgram)
            .RequireAuthorization("CoachOrPdr");

        endpoints.MapPost("me/students/{studentId}/programs-tasks/{taskId}",
                async Task<Results<Ok<StudentProgramDailyTaskInfo>, ProblemHttpResult>>
                ([FromQuery] Guid studentId, [FromQuery] Guid taskId,
                    [FromBody] UpdateStudentProgramDailyTaskRequest request, ClaimsPrincipal claimsPrincipal,
                    [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                    var coachId = claimsPrincipal.GetId();

                    var studentExamType = dbContext.StudentDetail
                        .Where(x => x.StudentId == studentId)
                        .Select(x => x.ExamType)
                        .FirstOrDefault();

                    if (request.ExamType == ProgramExamType.TYT)
                    {
                        
                    }
                    else if (request.ExamType == ProgramExamType.AYT)
                    {
                        
                    }
                    
                    var isLessonValid = LessonUtil.IsLessonValidForExamType(studentExamType, request.Lesson);

                    if (!isLessonValid)
                    {
                        var errorDesscriptor = new ErrorDescriptor()
                        {
                            Code = "LessonIsNotValid",
                            Description =
                                $"Girilen ders bilgisi öğrencinin sınav bilgisineuygun değil. Öğrenci Sınav Tipi: {studentExamType.ToString()}"
                        };

                        return errorDesscriptor.CreateProblem("Öğrenciye görev atanamadı!");
                    }

                    var studentProgramDailyTask = dbContext.StudentProgramDailyTasks.FirstOrDefault(x =>
                        x.Id == taskId && x.StudentProgramDaily.StudentId == studentId);

                    studentProgramDailyTask.Lesson = request.Lesson;
                    studentProgramDailyTask.ExamType = request.ExamType;
                    studentProgramDailyTask.TaskType = request.TaskType;
                    studentProgramDailyTask.Subject = request.Subject;
                    studentProgramDailyTask.Resource = request.Resource;
                    studentProgramDailyTask.Minute = request.Minute;
                    studentProgramDailyTask.QuestionCount = request.QuestionCount;
                    studentProgramDailyTask.Not = request.Not;
                    
                    dbContext.SaveChanges();

                    // TYT-Matematik-Soru/Test

                    var taskInfo = new StudentProgramDailyTaskInfo()
                    {
                        Id = studentProgramDailyTask.Id,
                        Title = $"{ProgramExamTypeUtil.GetName(studentProgramDailyTask.ExamType)} - {LessonUtil.GetName(studentProgramDailyTask.Lesson)} - {TaskTypeUtil.GetName(studentProgramDailyTask.TaskType)}",
                        ExamType = studentProgramDailyTask.ExamType,
                        Lesson = studentProgramDailyTask.Lesson,
                        TaskType = studentProgramDailyTask.TaskType,
                        Subject = studentProgramDailyTask.Subject,
                        Resource = studentProgramDailyTask.Resource,
                        Minute = studentProgramDailyTask.Minute,
                        QuestionCount = studentProgramDailyTask.QuestionCount,
                        State = studentProgramDailyTask.State,
                        Not = studentProgramDailyTask.Not,
                        Excuese = studentProgramDailyTask.Excuse,
                    };

                    return TypedResults.Ok(taskInfo);

                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachMeStudentsProgram)
            .RequireAuthorization("CoachOrPdr");

        endpoints.MapPost("me/students/{studentId}/programs-daily/{dailyId}",
                async Task<Results<Ok<StudentProgramDailyTaskInfo>, ProblemHttpResult>>
                ([FromQuery] Guid studentId, [FromQuery] Guid dailyId,
                    [FromBody] CreateStudentProgramDailyTaskRequest request, ClaimsPrincipal claimsPrincipal,
                    [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                    var coachId = claimsPrincipal.GetId();

                    var studentExamType = dbContext.StudentDetail
                        .Where(x => x.StudentId == studentId)
                        .Select(x => x.ExamType)
                        .FirstOrDefault();

                    if (request.ExamType == ProgramExamType.TYT)
                    {
                        
                    }
                    else if (request.ExamType == ProgramExamType.AYT)
                    {
                        
                    }
                    
                    var isLessonValid = LessonUtil.IsLessonValidForExamType(studentExamType, request.Lesson);

                    if (!isLessonValid)
                    {
                        var errorDesscriptor = new ErrorDescriptor()
                        {
                            Code = "LessonIsNotValid",
                            Description =
                                $"Girilen ders bilgisi öğrencinin sınav bilgisineuygun değil. Öğrenci Sınav Tipi: {studentExamType.ToString()}"
                        };

                        return errorDesscriptor.CreateProblem("Öğrenciye görev atanamadı!");
                    }

                    var studentProgramDailyTask = dbContext.StudentProgramDaily
                        .Include(x => x.DailyTasks)
                        .FirstOrDefault(x => x.Id == dailyId && x.StudentId == studentId);


                    var dailyTask = new StudentProgramDailyTasks()
                    {
                        Lesson = request.Lesson,
                        ExamType = request.ExamType,
                        TaskType = request.TaskType,
                        Subject = request.Subject,
                        Resource = request.Resource,
                        Minute = request.Minute,
                        QuestionCount = request.QuestionCount,
                        Not = request.Not,
                    };
                    
                    studentProgramDailyTask.DailyTasks.Add(dailyTask);
                    
                    dbContext.SaveChanges();

                    // TYT-Matematik-Soru/Test

                    var taskInfo = new StudentProgramDailyTaskInfo()
                    {
                        Id = dailyTask.Id,
                        Title = $"{ProgramExamTypeUtil.GetName(dailyTask.ExamType)} - {LessonUtil.GetName(dailyTask.Lesson)} - {TaskTypeUtil.GetName(dailyTask.TaskType)}",
                        ExamType = dailyTask.ExamType,
                        Lesson = dailyTask.Lesson,
                        TaskType = dailyTask.TaskType,
                        Subject = dailyTask.Subject,
                        Resource = dailyTask.Resource,
                        Minute = dailyTask.Minute,
                        QuestionCount = dailyTask.QuestionCount,
                        State = dailyTask.State,
                        Not = dailyTask.Not,
                        Excuese = dailyTask.Excuse,
                    };

                    return TypedResults.Ok(taskInfo);

                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachMeStudentsProgram)
            .RequireAuthorization("CoachOrPdr");

        
        endpoints.MapGet("me/students/{studentId}/programs-daily/{dailyId}/tasks/{taskId}",
                async Task<Results<Ok<StudentProgramDailyTaskInfo>, ProblemHttpResult>>
                ([FromQuery] Guid studentId, [FromQuery] Guid dailyId, [FromQuery] Guid taskId,
                    ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();
        
                    var coachId = claimsPrincipal.GetId();

                    var studentProgramDailyTask = dbContext.StudentProgramDailyTasks.FirstOrDefault(x =>
                        x.Id == taskId && x.StudentProgramDailyId == dailyId &&
                        x.StudentProgramDaily.StudentId == studentId);
        
                    var taskInfo = new StudentProgramDailyTaskInfo()
                    {
                        Id = studentProgramDailyTask.Id,
                        Title = $"{ProgramExamTypeUtil.GetName(studentProgramDailyTask.ExamType)} - {LessonUtil.GetName(studentProgramDailyTask.Lesson)} - {TaskTypeUtil.GetName(studentProgramDailyTask.TaskType)}",
                        ExamType = studentProgramDailyTask.ExamType,
                        Lesson = studentProgramDailyTask.Lesson,
                        TaskType = studentProgramDailyTask.TaskType,
                        Subject = studentProgramDailyTask.Subject,
                        Resource = studentProgramDailyTask.Resource,
                        Minute = studentProgramDailyTask.Minute,
                        QuestionCount = studentProgramDailyTask.QuestionCount,
                        State = studentProgramDailyTask.State,
                        Not = studentProgramDailyTask.Not,
                        Excuese = studentProgramDailyTask.Excuse,
                    };
        
                    return TypedResults.Ok(taskInfo);
        
                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachMeStudentsProgram)
            .RequireAuthorization("CoachOrPdr");
        
        endpoints.MapDelete("me/students/{studentId}/programs-tasks/{taskId}",
                async Task<Results<Ok, ProblemHttpResult>>
                ([FromQuery] Guid studentId, [FromQuery] Guid taskId, ClaimsPrincipal claimsPrincipal,
                    [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                    var coachId = claimsPrincipal.GetId();

                    var studentProgramDailyTask = dbContext.StudentProgramDailyTasks.FirstOrDefault(x =>
                        x.Id == taskId && x.StudentProgramDaily.StudentId == studentId);

                    dbContext.Remove(studentProgramDailyTask);
                    
                    dbContext.SaveChanges();

                    return TypedResults.Ok();

                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachMeStudentsProgram)
            .RequireAuthorization("CoachOrPdr");
    }

    public class StudentProgramInfo
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Text { get; set; }
        public List<StudentProgramWeeklyInfo> Weeklies { get; set; } = new();
    }

    public class StudentProgramWeeklyInfo
    {
        public Guid Id { get; set; }
    
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public string Text { get; set; }
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
    
        public TaskState State { get; set; }
        
        public TaskType TaskType { get; set; }

        public ProgramExamType ExamType { get; set; }
    
        public Lesson Lesson { get; set; }
        
        public string Subject { get; set; }
    }
    
    public class StudentProgramDailyTaskInfo
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }
    
        public TaskState State { get; set; }

        public ProgramExamType ExamType { get; set; }
    
        public Lesson Lesson { get; set; }
        
        public TaskType TaskType { get; set; }
        
        public string Subject { get; set; }
        
        public string Resource { get; set; }

        public ushort? QuestionCount { get; set; }
    
        public byte? Minute { get; set; }
    
        public string Not { get; set; }
        
        public string Excuese { get; set; }
    }
    
    public class CreateStudentProgramDailyTaskRequest
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ProgramExamType ExamType { get; set; }
    
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Lesson Lesson { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TaskType TaskType { get; set; }
        
        public string Subject { get; set; }
    
        public string Resource { get; set; }

        public ushort QuestionCount { get; set; }
    
        public byte Minute { get; set; }
    
        public string Not { get; set; }
    }
    
    public class UpdateStudentProgramDailyTaskRequest
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ProgramExamType ExamType { get; set; }
    
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Lesson Lesson { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TaskType TaskType { get; set; }
        
        public string Subject { get; set; }
    
        public string Resource { get; set; }

        public ushort QuestionCount { get; set; }
    
        public byte Minute { get; set; }
    
        public string Not { get; set; }
    }
}