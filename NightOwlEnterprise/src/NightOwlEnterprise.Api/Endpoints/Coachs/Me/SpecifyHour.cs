using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Endpoints.Coachs.Me;

public static class SpecifyHour
{
    public static void MapSpecifyHour(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/me/invitations/{invitationId}/specify-hour", async Task<Results<Ok, ProblemHttpResult>>
            ([FromRoute] Guid invitationId, [FromBody] SpecifyHourRequest specifyHourRequest,
                ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                var strCoachId = claimsPrincipal?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)
                                     ?.Value ??
                                 string.Empty;

                Guid.TryParse(strCoachId, out var coachId);

                var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                var invitationEntity = await dbContext.Invitations.Include(x => x.Student)
                    .Where(x => x.CoachId == coachId &&
                                x.Id == invitationId &&
                                x.State == InvitationState.SpecifyHour)
                    .FirstOrDefaultAsync();

                if (invitationEntity is null)
                {
                    var errorDescriptor = new ErrorDescriptor("NotFoundInvitation", "Randevu bulunamadı!");
                    return errorDescriptor.CreateProblem("Görüşme saati belirlenemedi!");
                }
                
                if (invitationEntity.Date < DateTime.UtcNow.ConvertUtcToTimeZone())
                {
                    var errorDescriptor = new ErrorDescriptor("InvitationDayExpired", "Randevu günü geçmiş!");
                    return errorDescriptor.CreateProblem("Görüşme saati belirlenemedi!");
                }

                invitationEntity.StartTime = specifyHourRequest.StartTime;
                invitationEntity.EndTime = specifyHourRequest.StartTime.Add(TimeSpan.FromHours(1));
                invitationEntity.State = InvitationState.WaitingApprove;

                try
                {
                    await dbContext.SaveChangesAsync();
                    
                    
                }
                catch (Exception e)
                {
                    var errorDescriptor = new ErrorDescriptor("NotSpecifiedHour", "Saat bilgisi atanamadı!");
                    return errorDescriptor.CreateProblem("Görüşme saati belirlenemedi!");
                }

                return TypedResults.Ok();
            }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
            .WithDescription("Koç görüşme saatini belirler").WithTags(TagConstants.CoachScheduling)
            .RequireAuthorization("CoachOrPdr");
    }
    
    public sealed class SpecifyHourRequest
    {
        public required TimeSpan StartTime { get; set; }
    }
}