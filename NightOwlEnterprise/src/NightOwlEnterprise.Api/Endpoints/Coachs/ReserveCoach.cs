using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using NightOwlEnterprise.Api.Endpoints.Coachs;
using NightOwlEnterprise.Api.Entities;
using NightOwlEnterprise.Api.Entities.Enums;
using NightOwlEnterprise.Api.Utils;
using Stripe;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace NightOwlEnterprise.Api.Endpoints.Coachs;

public static class ReserveCoach
{
    public static void MapReserveCoach(this IEndpointRouteBuilder endpoints)
    {
        //Öğrenci Koça Gün gönderiyor.
        endpoints.MapPost("/{coachId}/reserve", Results<Ok, ProblemHttpResult>
            ([FromRoute] Guid coachId, [FromBody] SendInvitationToCoachRequest inviteRequest,
                ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                var lockManager = sp.GetRequiredService<LockManager>();
                
                // ILoggerFactory örneği al
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                // ILogger örneğini oluştur
                var logger = loggerFactory.CreateLogger("Reserve");

                var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                var studentId = claimsPrincipal.GetId();

                var coach =  dbContext.Users
                    .Include(x => x.CoachDetail)
                    .Include(x => x.CoachStudentTrainingSchedules)
                    .FirstOrDefault(x => x.Id == coachId && x.UserType == UserType.Coach);

                if (coach is null)
                {
                    return TypedResults.Problem(new ProblemDetails()
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "Koç bulunamadı",
                    });
                }

                var dayQuota = inviteRequest.Day switch
                {
                    DayOfWeek.Sunday => coach.CoachDetail.SundayQuota,
                    DayOfWeek.Monday => coach.CoachDetail.MondayQuota,
                    DayOfWeek.Tuesday => coach.CoachDetail.TuesdayQuota,
                    DayOfWeek.Wednesday => coach.CoachDetail.WednesdayQuota,
                    DayOfWeek.Thursday => coach.CoachDetail.ThursdayQuota,
                    DayOfWeek.Friday => coach.CoachDetail.FridayQuota,
                    DayOfWeek.Saturday => coach.CoachDetail.SaturdayQuota,
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                var key = $"CoachId:{coachId},Day:{inviteRequest.Day}";
                var milliseconds = 10000;

                if (!lockManager.AcquireLock(key, milliseconds)) return TypedResults.Problem(new ProblemDetails()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Aynı gün için başkaları tarafından süreç başlatılmış ve devam ediyor.",
                });

                try
                {
                    var hasCoach = dbContext.CoachStudentTrainingSchedules.Any(x => x.StudentId == studentId);
                    //var hasCoach = coach.CoachStudentTrainingSchedules.Any(x => x.StudentId == studentId);
                    
                    if (hasCoach)
                    {
                        return new ErrorDescriptor("AlreadyHasCoach", "Koç seçimi yapılmış").CreateProblem(
                            "Koç Seçimi Yapılamadı!");
                    }
                    
                    var studentCountForDay = coach.CoachStudentTrainingSchedules.Count(x => x.VideoDay == inviteRequest.Day);

                    if (studentCountForDay >= dayQuota)
                    {
                        return new ErrorDescriptor("QuotaFull", "Belirtilen gün için koçun kontenjanı dolu!").CreateProblem(
                            "Koç seçilemedi!");
                    }
                    
                    var date = DateUtils.FindDate(inviteRequest.Day);

                    dbContext.CoachStudentTrainingSchedules.Add(new CoachStudentTrainingSchedule()
                    {
                        CoachId = coachId,
                        StudentId = studentId,
                        VideoDay = inviteRequest.Day,
                        VoiceDay = date.AddDays(3).DayOfWeek, 
                        CreatedAt = DateTime.UtcNow.ConvertUtcToTimeZone()
                    });

                    for (int i = 1; i <= 4; i++)
                    {
                        dbContext.Invitations.Add(new Invitation()
                        {
                            CoachId = coachId,
                            StudentId = studentId,
                            State = InvitationState.SpecifyHour,
                            Type = InvitationType.VideoCall,
                            Date = date,
                            Day = date.DayOfWeek,
                        });
                    
                        dbContext.Invitations.Add(new Invitation()
                        {
                            CoachId = coachId,
                            StudentId = studentId,
                            State = InvitationState.SpecifyHour,
                            Type = InvitationType.VoiceCall,
                            Date = date.AddDays(3),
                            Day = date.AddDays(3).DayOfWeek
                        });

                        date = date.AddDays(7);
                    }

                    var student = dbContext.StudentDetail.FirstOrDefault(x => x.StudentId == studentId);
                    
                    student.Status = StudentStatus.Active;

                    dbContext.SaveChanges();
                }
                catch (Exception e)
                {
                    logger.LogCritical(e,
                        "Reserve record not created. CoachId: {CoachId}, StudentId: {StudentId}, ReserveDay: {ReserveDay}",
                        coachId, studentId, inviteRequest.Day);
                    return new ErrorDescriptor("ReserveCoachFailed",
                            $"Mentör rezervasyonu yapılamadı. Mentör: {coachId.ToString()}, Öğrenci: {studentId.ToString()}, Gün: {inviteRequest.Day}")
                        .CreateProblem("Mentör rezervasyon İşlemi Başarısız");
                }
                finally
                {
                    lockManager.ReleaseLock(key);
                }

                return TypedResults.Ok();

            }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
            .WithTags(TagConstants.StudentsCoachListAndReserve).RequireAuthorization("Student");
    }

    public sealed class SendInvitationToCoachRequest
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DayOfWeek Day { get; set; }
    }
    
    public class SendInvitationToCoachRequestExamples : IMultipleExamplesProvider<SendInvitationToCoachRequest>
    {
        public IEnumerable<SwaggerExample<SendInvitationToCoachRequest>> GetExamples()
        {
            yield return SwaggerExample.Create("Pazartesi", new SendInvitationToCoachRequest()
            {
                Day = DayOfWeek.Monday
            });
            
            yield return SwaggerExample.Create("Salı", new SendInvitationToCoachRequest()
            {
                Day = DayOfWeek.Tuesday
            });
            
            yield return SwaggerExample.Create("Çarşamba", new SendInvitationToCoachRequest()
            {
                Day = DayOfWeek.Wednesday
            });
            
            yield return SwaggerExample.Create("Perşembe", new SendInvitationToCoachRequest()
            {
                Day = DayOfWeek.Thursday
            });
            
            yield return SwaggerExample.Create("Cuma", new SendInvitationToCoachRequest()
            {
                Day = DayOfWeek.Friday
            });
            
            yield return SwaggerExample.Create("Cumartesi", new SendInvitationToCoachRequest()
            {
                Day = DayOfWeek.Saturday
            });
            
            yield return SwaggerExample.Create("Pazar", new SendInvitationToCoachRequest()
            {
                Day = DayOfWeek.Sunday
            });
        }
    }
}