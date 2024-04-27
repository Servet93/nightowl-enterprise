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
using Stripe;
using Swashbuckle.AspNetCore.Annotations;

namespace NightOwlEnterprise.Api.Endpoints.Invitations.Student;

public static class Approve
{
    public static void MapApprove(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/{invitationId}/approve", async Task<Results<Ok, ProblemHttpResult>>
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
            
            invitationEntity.State = InvitationState.Approved;

            await dbContext.SaveChangesAsync();
            
            return TypedResults.Ok();
            
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithDescription("Öğrenci görüşme saatini onaylar").WithTags("Öğrenci Davetiye İşlemleri").RequireAuthorization("Student");
    }
}