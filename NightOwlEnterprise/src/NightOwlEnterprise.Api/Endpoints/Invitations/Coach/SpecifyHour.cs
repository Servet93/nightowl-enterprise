using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NightOwlEnterprise.Api.Endpoints.Invitations.Coach;

public static class SpecifyHour
{
    public static void MapSpecifyHour(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/{invitationId}/specify-hour", async Task<Results<Ok, ProblemHttpResult>>
            ([FromRoute] Guid invitationId, [FromBody]SpecifyHourRequest specifyHourRequest, ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
        {
            var strCoachId = claimsPrincipal?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            Guid.TryParse(strCoachId, out var coachId);
            
            var dbContext = sp.GetRequiredService<ApplicationDbContext>();
            
            var invitationEntity = await dbContext.Invitations.Include(x => x.Student)
                .Where(x => x.CoachId == coachId &&
                            x.Id == invitationId &&
                            x.State == InvitationState.SpecifyHour)
                .FirstOrDefaultAsync();

            if (invitationEntity is null)
            {
                return TypedResults.Problem("Davetiye bulunamadı!", statusCode: StatusCodes.Status400BadRequest);
            }

            invitationEntity.StartTime = specifyHourRequest.StartTime;
            invitationEntity.EndTime = specifyHourRequest.StartTime.Add(TimeSpan.FromHours(1));
            invitationEntity.State = InvitationState.WaitingApprove;
            invitationEntity.Type = specifyHourRequest.Type;

            await dbContext.SaveChangesAsync();
            
            return TypedResults.Ok();
            
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithDescription("Koç görüşme saatini belirler").WithTags("Koç").RequireAuthorization("Coach");
    }
    
    public sealed class SpecifyHourRequest
    {
        public TimeSpan StartTime { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public InvitationType Type { get; set; }
    }
}