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
using NightOwlEnterprise.Api.Services;
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
                    var hasCoach = coach.CoachStudentTrainingSchedules.Any(x => x.StudentId == studentId);

                    if (hasCoach)
                    {
                        return new ErrorDescriptor("AlreadyHasCoach", "Koç seçimi yapılmış").CreateProblem(
                            "Koç Seçimi Yapılamadı!");
                    }
                    
                    var studentCountForDay = coach.CoachStudentTrainingSchedules.Count(x => x.Day == inviteRequest.Day);

                    if (studentCountForDay >= dayQuota)
                    {
                        return new ErrorDescriptor("QuotaFull", "Belirtilen gün için koçun kontenjanı dolu!").CreateProblem(
                            "Koç seçilemedi!");
                    }

                    dbContext.CoachStudentTrainingSchedules.Add(new CoachStudentTrainingSchedule()
                    {
                        CoachId = coachId,
                        StudentId = studentId,
                        Day = inviteRequest.Day,
                        CreatedAt = DateTime.UtcNow
                    });

                    var date = FindDate(inviteRequest.Day);

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
                    
                    dbContext.SaveChanges();
                }
                catch (Exception e)
                {
                    logger.LogCritical(e,
                        "Reserve record not created. CoachId: {CoachId}, StudentId: {StudentId}, ReserveDay: {ReserveDay}",
                        coachId, studentId, inviteRequest.Day);
                }
                finally
                {
                    lockManager.ReleaseLock(key);
                }

                return TypedResults.Ok();

            }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
            .WithTags(TagConstants.StudentsCoachListAndReserve).RequireAuthorization("Student");

        static DateTime FindDate(DayOfWeek day)
        {
            // Örnek bir gün
            DayOfWeek verilenGun = day;

            // Bugünkü tarih
            DateTime bugun = DateTime.UtcNow;

            // Verilen günün bugünkü tarihle karşılaştırılması
            int gunFarki = ((int)verilenGun - (int)bugun.DayOfWeek + 7) % 7;

            // İleride mi, geride mi, yoksa bugün mü olduğunun kontrolü ve tarih hesaplaması
            DateTime bulunanTarih;
            if (gunFarki == 0) // Bugün
            {
                bulunanTarih = bugun;
            }
            else if (gunFarki > 0) // İleride
            {
                bulunanTarih = bugun.AddDays(gunFarki);
            }
            else // Geride
            {
                bulunanTarih = bugun.AddDays(7 + gunFarki);
            }

            return bulunanTarih;
        }
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