﻿using System.Globalization;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NightOwlEnterprise.Api.Endpoints.CommonDto;
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
                    ([FromRoute] Guid studentId, ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                    var coachId = claimsPrincipal.GetId();

                    var mountIndex = 1;
                    var weekIndex = 1;

                    var studentPrograms = await dbContext.StudentPrograms
                        .Where(x => x.CoachId == coachId && x.StudentId == studentId)
                        .Include(x => x.Weeklies)
                        .OrderBy(x => x.StartDate).ToListAsync();

                    var studentProgramInfoList = new List<StudentProgramInfo>();
                    
                    var cultureInfo = new CultureInfo("tr-TR");

                    bool? setCurrentProgram = null;
                    
                    bool? setCurrentWeekly = null;
                    
                    var now = DateTime.UtcNow.ConvertUtcToTimeZone();

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

                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachMeStudentsProgram)
            .RequireAuthorization("CoachOrPdr");
        
        endpoints.MapGet("me/students/{studentId}/programs/{programId}/days",
                async Task<Results<Ok<List<StudentProgramDayItemInfo>>, ProblemHttpResult>>
                    ([FromRoute] Guid studentId, [FromRoute] Guid programId, ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                    var coachId = claimsPrincipal.GetId();

                    var studentProgramDailies = await dbContext.StudentProgramWeekly
                        .Where(x => x.CoachId == coachId && x.StudentId == studentId && x.StudentProgramId == programId)
                        .Include(x => x.Dailies)
                        .SelectMany(x => x.Dailies)
                        .ToListAsync();

                    var studentProgramDayItemInfoList = new List<StudentProgramDayItemInfo>();
                    
                    studentProgramDailies.ForEach(x =>
                    {
                        studentProgramDayItemInfoList.Add(new StudentProgramDayItemInfo()
                        {
                            Id = x.Id,
                            Date = x.Date,
                            DateText = x.DateText,
                            DayText = x.DayText
                        });
                    });

                    studentProgramDayItemInfoList = studentProgramDayItemInfoList.OrderBy(x => x.Date).ToList();
                    
                    return TypedResults.Ok(studentProgramDayItemInfoList);

                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachMeStudentsProgram)
            .RequireAuthorization("CoachOrPdr");

        endpoints.MapGet("me/students/{studentId}/programs-week/{weekId}",
                async Task<Results<Ok<List<StudentProgramDayInfo>>, ProblemHttpResult>>
                ([FromRoute] Guid studentId, [FromRoute] Guid weekId, ClaimsPrincipal claimsPrincipal,
                    [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                    var coachId = claimsPrincipal.GetId();

                    var studentProgramDailyInfoList = await dbContext.StudentProgramDaily
                        .Where(x => x.StudentProgramWeeklyId == weekId && x.StudentId == studentId &&
                                    x.CoachId == coachId)
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

                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachMeStudentsProgram)
            .RequireAuthorization("CoachOrPdr");

        endpoints.MapPost("me/students/{studentId}/programs-tasks/{taskId}",
                async Task<Results<Ok<StudentProgramDayTaskInfo>, ProblemHttpResult>>
                ([FromRoute] Guid studentId, [FromRoute] Guid taskId,
                    [FromBody] UpdateStudentProgramDailyTaskDetailRequest request, ClaimsPrincipal claimsPrincipal,
                    [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                    var coachId = claimsPrincipal.GetId();

                    var studentExamType = dbContext.StudentDetail
                        .Where(x => x.StudentId == studentId)
                        .Select(x => x.ExamType)
                        .FirstOrDefault();

                    if (studentExamType == ExamType.TYT && request.ExamType == ProgramExamType.AYT)
                    {
                        var errorDescriptor = new ErrorDescriptor("CouldntBeAYTForTYTStudent",
                            "TYT öğrencisine AYT dersi atanamaz!");
                        return errorDescriptor.CreateProblem("Görev oluşturulamadı!");
                    }

                    var isLessonValid = false;

                    //Aşağıdaki if blokları TYT öğrencisine sadece TYT dersi atanabileceğini,
                    //AYT öğrencisine hem tyt hem ayt dersleri atanabileceğini göstermektedir.
                    if (studentExamType != ExamType.TYT && request.ExamType == ProgramExamType.TYT)
                    {
                        isLessonValid = LessonUtil.IsLessonValidForExamType(ExamType.TYT, request.Lesson);    
                    }
                    else if (studentExamType != ExamType.TYT && request.ExamType == ProgramExamType.AYT)
                    {
                        isLessonValid = LessonUtil.IsLessonValidForExamType(studentExamType, request.Lesson);
                    }
                    else if (studentExamType == ExamType.TYT && request.ExamType == ProgramExamType.TYT)
                    {
                        isLessonValid = LessonUtil.IsLessonValidForExamType(ExamType.TYT, request.Lesson);
                    }

                    if (!isLessonValid)
                    {
                        var errorDesscriptor = new ErrorDescriptor()
                        {
                            Code = "LessonIsNotValid",
                            Description =
                                $"Girilen ders bilgisi öğrencinin sınav bilgisine uygun değil. Öğrenci Sınav Tipi: {studentExamType.ToString()}"
                        };

                        return errorDesscriptor.CreateProblem("Öğrenci görevi güncellenemedi!");
                    }

                    var studentProgramDailyTask = await dbContext.StudentProgramDailyTasks.FirstOrDefaultAsync(x =>
                        x.Id == taskId && x.StudentProgramDaily.StudentId == studentId && x.StudentProgramDaily.CoachId == coachId);
                    
                    if (studentProgramDailyTask is null)
                    {
                        var errorDescriptor = new ErrorDescriptor()
                        {
                            Code = "TaskNotFound",
                            Description = "Öğrenciye ait görev bulunamadı!",
                        };

                        return errorDescriptor.CreateProblem("Öğrenci görevi güncellenemedi!'");
                    }


                    studentProgramDailyTask.Lesson = request.Lesson;
                    studentProgramDailyTask.ExamType = request.ExamType;
                    studentProgramDailyTask.TaskType = request.TaskType;
                    studentProgramDailyTask.Subject = request.Subject;
                    studentProgramDailyTask.Resource = request.Resource;
                    studentProgramDailyTask.EstimatedMinute = request.EstimatedMinute;
                    studentProgramDailyTask.QuestionCount = request.QuestionCount;
                    studentProgramDailyTask.Not = request.Not;
                    studentProgramDailyTask.UpdatedAt = DateTime.UtcNow.ConvertUtcToTimeZone();
                    
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
                        QuestionCount = studentProgramDailyTask.QuestionCount,
                        State = studentProgramDailyTask.State,
                        Not = studentProgramDailyTask.Not,
                        Excuese = studentProgramDailyTask.Excuse,
                    };

                    return TypedResults.Ok(taskInfo);

                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachMeStudentsProgram)
            .RequireAuthorization("CoachOrPdr");

        endpoints.MapPost("me/students/{studentId}/programs-daily/{dailyId}",
                async Task<Results<Ok<StudentProgramDayTaskInfo>, ProblemHttpResult>>
                ([FromRoute] Guid studentId, [FromRoute] Guid dailyId,
                    [FromBody] CreateStudentProgramDailyTaskRequest request, ClaimsPrincipal claimsPrincipal,
                    [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                    var coachId = claimsPrincipal.GetId();

                    var studentExamType = await dbContext.StudentDetail
                        .Where(x => x.StudentId == studentId)
                        .Select(x => x.ExamType)
                        .FirstOrDefaultAsync();

                    if (studentExamType == ExamType.TYT && request.ExamType == ProgramExamType.AYT)
                    {
                        var errorDescriptor = new ErrorDescriptor("CouldntBeAYTForTYTStudent",
                            "TYT öğrencisine AYT dersi atanamaz!");
                        return errorDescriptor.CreateProblem("Görev oluşturulamadı!");
                    }

                    var isLessonValid = false;

                    //Aşağıdaki if blokları TYT öğrencisine sadece TYT dersi atanabileceğini,
                    //AYT öğrencisine hem tyt hem ayt dersleri atanabileceğini göstermektedir.
                    if (studentExamType != ExamType.TYT && request.ExamType == ProgramExamType.TYT)
                    {
                        isLessonValid = LessonUtil.IsLessonValidForExamType(ExamType.TYT, request.Lesson);    
                    }
                    else if (studentExamType != ExamType.TYT && request.ExamType == ProgramExamType.AYT)
                    {
                        isLessonValid = LessonUtil.IsLessonValidForExamType(studentExamType, request.Lesson);
                    }
                    else if (studentExamType == ExamType.TYT && request.ExamType == ProgramExamType.TYT)
                    {
                        isLessonValid = LessonUtil.IsLessonValidForExamType(ExamType.TYT, request.Lesson);
                    }

                    if (!isLessonValid)
                    {
                        var errorDesscriptor = new ErrorDescriptor()
                        {
                            Code = "LessonIsNotValid",
                            Description =
                                $"Girilen ders bilgisi öğrencinin sınav bilgisine uygun değil. Öğrenci Sınav Tipi: {studentExamType.ToString()}"
                        };

                        return errorDesscriptor.CreateProblem("Öğrenciye görev atanamadı!");
                    }

                    var studentProgramDailyTask = await dbContext.StudentProgramDaily
                        .Include(x => x.DailyTasks)
                        .FirstOrDefaultAsync(x => x.Id == dailyId && x.StudentId == studentId && x.CoachId == coachId);
                    
                    if (studentProgramDailyTask is null)
                    {
                        var errorDescriptor = new ErrorDescriptor()
                        {
                            Code = "TaskNotFound",
                            Description = "Öğrenciye ait görev bulunamadı!",
                        };

                        return errorDescriptor.CreateProblem("Öğrenciye görev atanamadı!'");
                    }

                    
                    var dailyTask = new StudentProgramDailyTasks()
                    {
                        Lesson = request.Lesson,
                        ExamType = request.ExamType,
                        TaskType = request.TaskType,
                        Subject = request.Subject,
                        Resource = request.Resource,
                        EstimatedMinute = request.EstimatedMinute,
                        QuestionCount = request.QuestionCount,
                        Not = request.Not,
                        CreatedAt = DateTime.UtcNow.ConvertUtcToTimeZone(),
                    };
                    
                    studentProgramDailyTask.DailyTasks.Add(dailyTask);
                    
                    dbContext.SaveChanges();

                    // TYT-Matematik-Soru/Test

                    var taskInfo = new StudentProgramDayTaskInfo()
                    {
                        Id = dailyTask.Id,
                        Title = $"{ProgramExamTypeUtil.GetName(dailyTask.ExamType)} - {LessonUtil.GetName(dailyTask.Lesson)} - {TaskTypeUtil.GetName(dailyTask.TaskType)}",
                        ExamType = dailyTask.ExamType,
                        Lesson = dailyTask.Lesson,
                        TaskType = dailyTask.TaskType,
                        Subject = dailyTask.Subject,
                        Resource = dailyTask.Resource,
                        EstimatedMinute = dailyTask.EstimatedMinute,
                        QuestionCount = dailyTask.QuestionCount,
                        State = dailyTask.State,
                        Not = dailyTask.Not,
                        Excuese = dailyTask.Excuse,
                    };

                    return TypedResults.Ok(taskInfo);

                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachMeStudentsProgram)
            .RequireAuthorization("CoachOrPdr");
        
        endpoints.MapGet("me/students/{studentId}/programs-tasks/{taskId}",
                async Task<Results<Ok<StudentProgramDayTaskInfo>, ProblemHttpResult>>
                ([FromRoute] Guid studentId, [FromRoute] Guid taskId,
                    ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();
        
                    var coachId = claimsPrincipal.GetId();

                    var studentProgramDailyTask = await dbContext.StudentProgramDailyTasks.FirstOrDefaultAsync(x =>
                        x.Id == taskId && x.StudentProgramDaily.StudentId == studentId &&
                        x.StudentProgramDaily.CoachId == coachId);

                    if (studentProgramDailyTask is null)
                    {
                        var errorDescriptor = new ErrorDescriptor()
                        {
                            Code = "TaskNotFound",
                            Description = "Öğrenciye ait görev bulunamadı!",
                        };

                        return errorDescriptor.CreateProblem("Öğrenci görev detayı getirilemedi!'");
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
        
                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachMeStudentsProgram)
            .RequireAuthorization("CoachOrPdr");
        
        endpoints.MapDelete("me/students/{studentId}/programs-tasks/{taskId}",
                async Task<Results<Ok, ProblemHttpResult>>
                ([FromRoute] Guid studentId, [FromRoute] Guid taskId, ClaimsPrincipal claimsPrincipal,
                    [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                    var coachId = claimsPrincipal.GetId();

                    var studentProgramDailyTask = await dbContext.StudentProgramDailyTasks.FirstOrDefaultAsync(x =>
                        x.Id == taskId && x.StudentProgramDaily.StudentId == studentId && x.StudentProgramDaily
                            .CoachId == coachId);
                    
                    if (studentProgramDailyTask is null)
                    {
                        var errorDescriptor = new ErrorDescriptor()
                        {
                            Code = "TaskNotFound",
                            Description = "Öğrenciye ait görev bulunamadı!",
                        };

                        return errorDescriptor.CreateProblem("Öğrenci görevi silinemedi!'");
                    }

                    dbContext.Remove(studentProgramDailyTask);
                    
                    dbContext.SaveChanges();

                    return TypedResults.Ok();

                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachMeStudentsProgram)
            .RequireAuthorization("CoachOrPdr");
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
    
        public ushort EstimatedMinute { get; set; }
    
        public string Not { get; set; }
    }
    
    public class UpdateStudentProgramDailyTaskDetailRequest
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
    
        public byte EstimatedMinute { get; set; }
    
        public string Not { get; set; }
    }
}