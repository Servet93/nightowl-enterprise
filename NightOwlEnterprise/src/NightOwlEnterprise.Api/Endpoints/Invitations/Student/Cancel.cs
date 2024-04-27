using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NightOwlEnterprise.Api.Endpoints.Invitations.Student;

public static class Cancel
{
    public static void MapCancel(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/{invitationId}/cancel", async Task<Results<Ok, ProblemHttpResult>>
            ([FromRoute] Guid invitationId, ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
        {
            var strCoachId = claimsPrincipal?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            Guid.TryParse(strCoachId, out var coachId);
            
            var dbContext = sp.GetRequiredService<ApplicationDbContext>();
            
            var invitationEntity = await dbContext.Invitations.Include(x => x.Student)
                .Where(x => x.CoachId == coachId &&
                            x.Id == invitationId &&
                            x.State == InvitationState.WaitingApprove)
                .FirstOrDefaultAsync();

            if (invitationEntity is null)
            {
                return TypedResults.Problem("Davetiye bulunamadı!", statusCode: StatusCodes.Status400BadRequest);
            }

            
            invitationEntity.State = InvitationState.Cancelled;

            await dbContext.SaveChangesAsync();
            
            return TypedResults.Ok();
            
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithDescription("Öğrenci görüşme saatini reddeder").WithTags("Öğrenci Davetiye İşlemleri").RequireAuthorization("Student");
    }
}