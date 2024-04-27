using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
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

namespace NightOwlEnterprise.Api.Endpoints.Invitations.Student;

public static class SendInvitationToCoach
{
    public static void MapSendInvitationToCoach(this IEndpointRouteBuilder endpoints)
    {
        //Öğrenci Koça Gün gönderiyor.
        endpoints.MapPost("/{coachId}/invite", Results<Ok, ProblemHttpResult>
            ([FromRoute] Guid coachId, [FromBody] SendInvitationToCoachRequest inviteRequest,
                ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                var lockManager = sp.GetRequiredService<LockManager>();
                
                // ILoggerFactory örneği al
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                // ILogger örneğini oluştur
                var logger = loggerFactory.CreateLogger("Invite");

                var dbContext = sp.GetRequiredService<ApplicationDbContext>();
                
                var strStudentUserId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

                Guid.TryParse(strStudentUserId, out var studentUserId);

                var coach =  dbContext.Users.Include(x => x.CoachDetail)
                    .FirstOrDefault(x => x.Id == coachId && x.UserType == UserType.Coach);

                if (coach is null)
                {
                    return TypedResults.Problem(new ProblemDetails()
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "Koç bulunamadı",
                    });
                }

                //Tarih bu hafta aralığında mı
                var weekDays = DateTime.Now.GetWeekDays();
                
                if (weekDays.Contains(inviteRequest.Date))
                {
                    return TypedResults.Problem(new ProblemDetails()
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "Randevu günü mevcut haftanın dışında!",
                    });
                }
                
                if (inviteRequest.Date < DateTime.Now.AddDays(-1))
                {
                    return TypedResults.Problem(new ProblemDetails()
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "Randevu günü geçmiş tarihli olamaz!",
                    });
                }

                //aralığındaysa kontenjan var mı
                var dayOfWeek = inviteRequest.Date.DayOfWeek;

                var quota = dayOfWeek switch
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

                var key = $"CoachId:{coachId},Date:{inviteRequest.Date.ToShortDateString()}";
                var milliseconds = 10000;

                if (!lockManager.AcquireLock(key, milliseconds)) return TypedResults.Problem(new ProblemDetails()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Aynı tarih için başkaları tarafından süreç başlatılmış",
                });

                try
                {
                    var inviteCountForDate =
                        dbContext.Invitations.Where(x => x.Date == inviteRequest.Date).Count();

                    if (inviteCountForDate >= quota)
                        return TypedResults.Problem(new ProblemDetails()
                        {
                            Status = StatusCodes.Status400BadRequest,
                            Detail = "Koçun takvimi dolu",
                        });

                    dbContext.Invitations.Add(new Invitation()
                    {
                        CoachId = coachId,
                        StudentId = studentUserId,
                        Date = inviteRequest.Date,
                        State = InvitationState.SpecifyHour,
                        Type = InvitationType.VideoCall,
                    });
                    
                    dbContext.SaveChanges();
                }
                catch (Exception e)
                {
                    logger.LogCritical(e,
                        "Invite record not created. CoachId: {CoachId}, StudentId: {StudentId}, InviteDate: {InviteDate}",
                        coachId, studentUserId, inviteRequest.Date);
                }
                finally
                {
                    lockManager.ReleaseLock(key);
                }

                return TypedResults.Ok();

            }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
            .WithTags("Öğrenci Davetiye İşlemleri").RequireAuthorization();
    }

    public sealed class SendInvitationToCoachRequest
    {
        public DateTime Date { get; set; }
    }
    
    public class SendInvitationToCoachRequestExamples : IMultipleExamplesProvider<SendInvitationToCoachRequest>
    {
        
        public IEnumerable<SwaggerExample<SendInvitationToCoachRequest>> GetExamples()
        {
            var weekDays = DateTime.Now.GetWeekDays();
            
            yield return SwaggerExample.Create("Sunday -> " + weekDays[0].Date.ToShortDateString(), new SendInvitationToCoachRequest()
            {
                Date = weekDays[0].Date
            });
            
            yield return SwaggerExample.Create("Monday -> " + weekDays[1].Date.ToShortDateString(), new SendInvitationToCoachRequest()
            {
                Date = weekDays[1].Date
            });
            
            yield return SwaggerExample.Create("Tuesday -> " + weekDays[2].Date.ToShortDateString(), new SendInvitationToCoachRequest()
            {
                Date = weekDays[2].Date
            });
            
            yield return SwaggerExample.Create("Wednesday -> " + weekDays[3].Date.ToShortDateString(), new SendInvitationToCoachRequest()
            {
                Date = weekDays[3].Date
            });
            
            yield return SwaggerExample.Create("Thursday -> " + weekDays[4].Date.ToShortDateString(), new SendInvitationToCoachRequest()
            {
                Date = weekDays[4].Date
            });
            
            yield return SwaggerExample.Create("Friday -> " + weekDays[5].Date.ToShortDateString(), new SendInvitationToCoachRequest()
            {
                Date = weekDays[4].Date
            });
            
            yield return SwaggerExample.Create("Saturday -> " + weekDays[6].Date.ToShortDateString(), new SendInvitationToCoachRequest()
            {
                Date = weekDays[6].Date
            });
        }
    }
}