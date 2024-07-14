using System.Globalization;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NightOwlEnterprise.Api.Endpoints.CommonDto;
using NightOwlEnterprise.Api.Entities;
using NightOwlEnterprise.Api.Entities.Enums;
using NightOwlEnterprise.Api.Utils;

namespace NightOwlEnterprise.Api.Endpoints.Students.Me;

public static class Program
{
    public static void MapProgram(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("me/programs/",
                async Task<Results<Ok<List<StudentProgramInfo>>, ProblemHttpResult>>
                    (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                    var studentId = claimsPrincipal.GetId();

                    var mountIndex = 1;
                    var weekIndex = 1;
                    
                    var now = DateTime.UtcNow.ConvertUtcToTimeZone();
                    
                    var studentPrograms = await dbContext.StudentPrograms
                        .Where(x => x.StudentId == studentId)
                        .Include(x => x.Weeklies)
                        .OrderBy(x => x.StartDate).ToListAsync();

                    var studentProgramInfoList = new List<StudentProgramInfo>();

                    var cultureInfo = new CultureInfo("tr-TR");

                    bool? setCurrentProgram = null;
                    
                    bool? setCurrentWeekly = null;

                    foreach (var studentProgram in studentPrograms)
                    {
                        var startDateText = studentProgram.StartDate.ToString("dd MMMM \\/ yyyy", cultureInfo);
                        var endDateText = studentProgram.EndDate.ToString("dd MMMM \\/ yyyy", cultureInfo);
                        
                        var studentProgramInfo = new StudentProgramInfo()
                        {
                            Id = studentProgram.Id,
                            StartDate = studentProgram.StartDate,
                            EndDate = studentProgram.EndDate,
                            Text =
                                $"{(mountIndex).ToString()}.Ay ({startDateText} - {endDateText})",
                        };

                        if (!setCurrentProgram.HasValue &&
                            (
                                (studentProgram.StartDate <= now && studentProgram.EndDate >= now) ||
                                (studentProgram.StartDate >= now && studentProgram.EndDate >= now)
                            ))
                        {
                            setCurrentProgram = true;
                            studentProgramInfo.IsCurrent = true;
                        }

                        foreach (var weeklyInfo in studentProgram.Weeklies.OrderBy(x => x.StartDate))
                        {
                            var weekStartDateText = weeklyInfo.StartDate.ToString("dd MMMM \\/ yyyy", cultureInfo);
                            var weekEndDateText = weeklyInfo.EndDate.ToString("dd MMMM \\/ yyyy", cultureInfo);

                            var week = new StudentProgramWeekInfo()
                            {
                                Id = weeklyInfo.Id,
                                StartDate = weeklyInfo.StartDate,
                                EndDate = weeklyInfo.EndDate,
                                Text = $"{(weekIndex).ToString()}.Hafta ({weekStartDateText} - {weekEndDateText})",
                            };
                            
                            studentProgramInfo.Weeklies.Add(week);
                            
                            if (setCurrentProgram.HasValue && !setCurrentWeekly.HasValue &&
                                (
                                    (weeklyInfo.StartDate <= now && weeklyInfo.EndDate >= now) ||
                                    (weeklyInfo.StartDate >= now && weeklyInfo.EndDate >= now)
                                ))
                            {
                                setCurrentWeekly = true;
                                week.IsCurrent = true;
                            }

                            weekIndex++;
                        }

                        mountIndex++;
                        weekIndex = 1;

                        studentProgramInfoList.Add(studentProgramInfo);
                    }

                    return TypedResults.Ok(studentProgramInfoList);

                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.StudentsMeProgram)
            .RequireAuthorization("Student");
        
        endpoints.MapGet("me/programs-week/{weekId}",
                async Task<Results<Ok<List<StudentProgramDayInfo>>, ProblemHttpResult>>
                ([FromRoute] Guid weekId, ClaimsPrincipal claimsPrincipal,
                    [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();
                    
                    var studentId = claimsPrincipal.GetId();
                    
                    var studentProgramDailyInfoList = await dbContext.StudentProgramDaily
                        .Where(x => x.StudentProgramWeeklyId == weekId && x.StudentId == studentId)
                        .Include(x => x.DailyTasks)
                        .OrderBy(x => x.Date)
                        .Select(x => new StudentProgramDayInfo()
                        {
                            Id = x.Id,
                            Date = x.Date,
                            DateText = x.DateText,
                            DayText = x.DayText,
                            DayOfMonth = x.Date.Day,
                            Tasks = x.DailyTasks.Select(y => new StudentProgramTaskSummarizedInfo()
                            {
                                Id = y.Id,
                                Lesson = y.Lesson,
                                State = y.State,
                                Subject = y.Subject,
                                ExamType = y.ExamType
                            }).ToList()
                        })
                        .ToListAsync();
                    
                    studentProgramDailyInfoList.ForEach(x =>
                    {
                        x.Tasks.ForEach(y =>
                        {
                            y.Title =
                                $"{ProgramExamTypeUtil.GetName(y.ExamType)} - {LessonUtil.GetName(y.Lesson)} - {TaskTypeUtil.GetName(y.TaskType)}";
                        });
                    });

                    return TypedResults.Ok(studentProgramDailyInfoList);

                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.StudentsMeProgram)
            .RequireAuthorization("Student");

        endpoints.MapPost("me/programs-tasks/{taskId}",
                async Task<Results<Ok<StudentProgramDayTaskInfo>, ProblemHttpResult>>
                ([FromRoute] Guid taskId,
                    [FromBody] UpdateStudentProgramDailyTaskStateRequest request, ClaimsPrincipal claimsPrincipal,
                    [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                    var studentId = claimsPrincipal.GetId();

                    if (request.CompletedMinute <= 0)
                    {
                        var errorDescriptor = new ErrorDescriptor()
                        {
                            Code = "CompletedTimeInvalid",
                            Description = "Bitirme süresi 0'dan büyük olmalıdır!"
                        };

                        return errorDescriptor.CreateProblem("Görev durumu güncellenemedi!");
                    }

                    var studentProgramDailyTask = await dbContext.StudentProgramDailyTasks.FirstOrDefaultAsync(x =>
                        x.Id == taskId && x.StudentProgramDaily.StudentId == studentId);

                    if (studentProgramDailyTask is null)
                    {
                        var errorDescriptor = new ErrorDescriptor()
                        {
                            Code = "TaskNotFound",
                            Description = "Öğrenciye ait görev bulunamadı!"
                        };

                        return errorDescriptor.CreateProblem("Görev durumu güncellenemedi!");
                    }

                    if (string.IsNullOrEmpty(request.Excuse))
                    {
                        var errorDescriptor = new ErrorDescriptor()
                        {
                            Code = "ExcuseNotBeEmpty",
                            Description = "Mazeretinizi giriniz!"
                        };

                        return errorDescriptor.CreateProblem("Görev durumu güncellenemedi!");
                    }

                    studentProgramDailyTask.Excuse = request.Excuse;
                    studentProgramDailyTask.State = request.CompletedState == CompletedState.Done ? TaskState.Done : TaskState.PartiallyDone;
                    studentProgramDailyTask.CompletedMinute = request.CompletedMinute;
                    studentProgramDailyTask.DoneTime = DateTime.UtcNow.ConvertUtcToTimeZone();

                    dbContext.SaveChanges();

                    // TYT-Matematik-Soru/Test

                    var taskInfo = new StudentProgramDayTaskInfo()
                    {
                        Id = studentProgramDailyTask.Id,
                        Title = $"{ProgramExamTypeUtil.GetName(studentProgramDailyTask.ExamType)} - {LessonUtil.GetName(studentProgramDailyTask.Lesson)} - {TaskTypeUtil.GetName(studentProgramDailyTask.TaskType)}",
                        ExamType = studentProgramDailyTask.ExamType,
                        Lesson = studentProgramDailyTask.Lesson,
                        TaskType = studentProgramDailyTask.TaskType,
                        Subject = studentProgramDailyTask.Subject,
                        Resource = studentProgramDailyTask.Resource,
                        EstimatedMinute = studentProgramDailyTask.EstimatedMinute,
                        CompletedMinute = studentProgramDailyTask.CompletedMinute,
                        QuestionCount = studentProgramDailyTask.QuestionCount,
                        State = studentProgramDailyTask.State,
                        Not = studentProgramDailyTask.Not,
                        Excuese = studentProgramDailyTask.Excuse,
                    };

                    return TypedResults.Ok(taskInfo);

                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.StudentsMeProgram)
            .RequireAuthorization("Student");
        
        endpoints.MapGet("me/programs-tasks/{taskId}",
                async Task<Results<Ok<StudentProgramDayTaskInfo>, ProblemHttpResult>>
                ([FromRoute] Guid taskId, ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();
        
                    var studentId = claimsPrincipal.GetId();

                    var studentProgramDailyTask = await dbContext.StudentProgramDailyTasks.FirstOrDefaultAsync(x =>
                        x.Id == taskId && x.StudentProgramDaily.StudentId == studentId);
                    
                    if (studentProgramDailyTask is null)
                    {
                        var errorDescriptor = new ErrorDescriptor()
                        {
                            Code = "TaskNotFound",
                            Description = "Öğrenciye ait görev bulunamadı!"
                        };

                        return errorDescriptor.CreateProblem("Görev durumu güncellenemedi!");
                    }
        
                    var taskInfo = new StudentProgramDayTaskInfo()
                    {
                        Id = studentProgramDailyTask.Id,
                        Title = $"{ProgramExamTypeUtil.GetName(studentProgramDailyTask.ExamType)} - {LessonUtil.GetName(studentProgramDailyTask.Lesson)} - {TaskTypeUtil.GetName(studentProgramDailyTask.TaskType)}",
                        ExamType = studentProgramDailyTask.ExamType,
                        Lesson = studentProgramDailyTask.Lesson,
                        TaskType = studentProgramDailyTask.TaskType,
                        Subject = studentProgramDailyTask.Subject,
                        Resource = studentProgramDailyTask.Resource,
                        EstimatedMinute = studentProgramDailyTask.EstimatedMinute,
                        CompletedMinute = studentProgramDailyTask.CompletedMinute,
                        QuestionCount = studentProgramDailyTask.QuestionCount,
                        State = studentProgramDailyTask.State,
                        Not = studentProgramDailyTask.Not,
                        Excuese = studentProgramDailyTask.Excuse,
                    };
        
                    return TypedResults.Ok(taskInfo);
        
                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.StudentsMeProgram)
            .RequireAuthorization("Student");
        
          endpoints.MapGet("me/programs-daily-tasks",
                async Task<Results<Ok<StudentProgramDailyInfo>, ProblemHttpResult>>
                (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();
        
                    var studentId = claimsPrincipal.GetId();

                    var now = DateTime.UtcNow.ConvertUtcToTimeZone();

                    var dailyId = await dbContext.StudentProgramDaily.Where(x => x.StudentId == studentId && x.Date.Date == now.Date)
                        .Select(x => x.Id).FirstOrDefaultAsync();
                    
                    var studentProgramDailyTasks = await dbContext.StudentProgramDailyTasks
                        .Where(x => x.StudentProgramDailyId == dailyId)
                        .Select(x => new
                        {
                            Id = x.Id,
                            ExamType = x.ExamType,
                            Lesson = x.Lesson,
                            TaskType = x.TaskType,
                            State = x.State,
                            Subject = x.Subject
                        }).ToListAsync();

                    var studentProgramDailyTaskSummarizedInfoList = new List<StudentProgramTaskSummarizedInfo>();
                    
                    studentProgramDailyTasks.ForEach(x =>
                    {
                        studentProgramDailyTaskSummarizedInfoList.Add(new StudentProgramTaskSummarizedInfo()
                        {
                            Id = x.Id,
                            Title = $"{ProgramExamTypeUtil.GetName(x.ExamType)} - {LessonUtil.GetName(x.Lesson)} - {TaskTypeUtil.GetName(x.TaskType)}",
                            Lesson = x.Lesson,
                            State = x.State,
                            Subject = x.Subject,
                            ExamType = x.ExamType,
                            TaskType = x.TaskType
                        });
                    });

                    var dailyInfo = new StudentProgramDailyInfo()
                    {
                        TotalTask = (byte)studentProgramDailyTaskSummarizedInfoList.Count,
                        CompletedTask =
                            (byte)studentProgramDailyTaskSummarizedInfoList.Count(x => x.State == TaskState.Done),
                        PartialCompletedTask = (byte)studentProgramDailyTaskSummarizedInfoList.Count(x => x.State == TaskState.PartiallyDone),
                        DailyTaskSummarizedInfos = studentProgramDailyTaskSummarizedInfoList
                    };
                    
                    return TypedResults.Ok(dailyInfo);
        
                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.StudentsMeProgram)
            .RequireAuthorization("Student");
    }
    
    public class UpdateStudentProgramDailyTaskStateRequest
    {
        public ushort CompletedMinute { get; set; }
        
        public string Excuse { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CompletedState CompletedState { get; set; }
    }

    public enum CompletedState
    {
        Done,
        PartialDone
    }
}