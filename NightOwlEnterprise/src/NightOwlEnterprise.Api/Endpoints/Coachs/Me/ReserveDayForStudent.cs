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

namespace NightOwlEnterprise.Api.Endpoints.Coachs.Me;

public static class MeReserveDayForStudent
{
    public static void MapMeReserveDayForStudent(this IEndpointRouteBuilder endpoints)
    {
        //Öğrenci Koça Gün gönderiyor.
        endpoints.MapPost("me/students/{studentId}/reserve-day", Results<Ok, ProblemHttpResult>
            ([FromRoute] Guid studentId, [FromBody] SpecifyDayForStudentRequest inviteRequest,
                ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                var lockManager = sp.GetRequiredService<LockManager>();
                
                // ILoggerFactory örneği al
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                // ILogger örneğini oluştur
                var logger = loggerFactory.CreateLogger("ReserveDayForStudent");

                var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                var coachId = claimsPrincipal.GetId();

                var coach =  dbContext.Users
                    .Include(x => x.CoachDetail)
                    .Include(x => x.CoachStudentTrainingSchedules)
                    .FirstOrDefault(x => x.Id == coachId && x.UserType == UserType.Pdr);

                if (coach is null)
                {
                    return TypedResults.Problem(new ProblemDetails()
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "Pdr bulunamadı",
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

                if (dayQuota == 0)
                {
                    return new ErrorDescriptor("QuotaEmpty", "Belirtilen gün için pdr'nin kontenjanı 0!").CreateProblem(
                        "Gün seçimi yapılamadı!");
                }
                
                var key = $"CoachId:{coachId},Day:{inviteRequest.Day}";
                var milliseconds = 10000;

                if (!lockManager.AcquireLock(key, milliseconds))
                {
                    return new ErrorDescriptor("ReserveDayForStudentContinue",
                        "Aynı gün için başkaları tarafından süreç başlatılmış ve devam ediyor.").CreateProblem(
                        "Gün Seçimi Yapılamadı!");
                }

                try
                {
                    var hasCoach = coach.CoachStudentTrainingSchedules.Any(x => x.StudentId == studentId && x.Day.HasValue);

                    if (hasCoach)
                    {
                        return new ErrorDescriptor("AlreadyReservedDayForStudent", "Pdr tarafından daha önce gün ataması yapılmış").CreateProblem(
                            "Gün Seçimi Yapılamadı!");
                    }
                    
                    var studentCountForDay = coach.CoachStudentTrainingSchedules
                        .Where(x => x.Day.HasValue && x.Day == inviteRequest.Day)
                        .Count();

                    if (studentCountForDay >= dayQuota)
                    {
                        return new ErrorDescriptor("QuotaFull", "Belirtilen gün için pdr'nin kontenjanı dolu!").CreateProblem(
                            "Gün seçimi yapılamadı!");
                    }

                    var coachStudentTrainingSchedule = dbContext.CoachStudentTrainingSchedules.FirstOrDefault(x =>
                        x.StudentId == studentId && x.CoachId == coachId && !x.Day.HasValue);

                    coachStudentTrainingSchedule.Day = inviteRequest.Day;

                    var date = DateUtils.FindDate(inviteRequest.Day);

                    for (int i = 1; i <= 4; i++)
                    {
                        dbContext.Invitations.Add(new Invitation()
                        {
                            CoachId = coachId,
                            StudentId = studentId,
                            State = InvitationState.SpecifyHour,
                            Type = InvitationType.VideoCall,
                            Date = date,
                        });
                    
                        dbContext.Invitations.Add(new Invitation()
                        {
                            CoachId = coachId,
                            StudentId = studentId,
                            State = InvitationState.SpecifyHour,
                            Type = InvitationType.VoiceCall,
                            Date = date.AddDays(3),
                        });

                        date = date.AddDays(7);
                    }

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
            .WithTags(TagConstants.PdrMeReservationDay).RequireAuthorization("Pdr");
    }

    public sealed class SpecifyDayForStudentRequest
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DayOfWeek Day { get; set; }
    }
    
    public class SpecifyDayForStudentRequestExamples : IMultipleExamplesProvider<SpecifyDayForStudentRequest>
    {
        public IEnumerable<SwaggerExample<SpecifyDayForStudentRequest>> GetExamples()
        {
            yield return SwaggerExample.Create("Pazartesi", new SpecifyDayForStudentRequest()
            {
                Day = DayOfWeek.Monday
            });
            
            yield return SwaggerExample.Create("Salı", new SpecifyDayForStudentRequest()
            {
                Day = DayOfWeek.Tuesday
            });
            
            yield return SwaggerExample.Create("Çarşamba", new SpecifyDayForStudentRequest()
            {
                Day = DayOfWeek.Wednesday
            });
            
            yield return SwaggerExample.Create("Perşembe", new SpecifyDayForStudentRequest()
            {
                Day = DayOfWeek.Thursday
            });
            
            yield return SwaggerExample.Create("Cuma", new SpecifyDayForStudentRequest()
            {
                Day = DayOfWeek.Friday
            });
            
            yield return SwaggerExample.Create("Cumartesi", new SpecifyDayForStudentRequest()
            {
                Day = DayOfWeek.Saturday
            });
            
            yield return SwaggerExample.Create("Pazar", new SpecifyDayForStudentRequest()
            {
                Day = DayOfWeek.Sunday
            });
        }
    }
}