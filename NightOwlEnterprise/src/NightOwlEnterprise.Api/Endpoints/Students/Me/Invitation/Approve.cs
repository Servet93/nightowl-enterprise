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
using NightOwlEnterprise.Api.Entities;
using NightOwlEnterprise.Api.Entities.Enums;
using Stripe;
using Swashbuckle.AspNetCore.Annotations;

namespace NightOwlEnterprise.Api.Endpoints.Students.Me.Invitation;

public static class Approve
{
    public static void MapApprove(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/me/invitations/{invitationId}/approve", async Task<Results<Ok, ProblemHttpResult>>
                ([FromRoute] Guid invitationId, ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                var studentId = claimsPrincipal.GetId();

                var dbContext = sp.GetRequiredService<ApplicationDbContext>();
                var zoom = sp.GetRequiredService<Zoom>();

                var invitationEntity = await dbContext.Invitations
                    .Include(x => x.Student)
                    .Include(x => x.Student.StudentDetail)
                    .Include(x => x.Coach)
                    .Include(x => x.Coach.CoachDetail)
                    .Where(x => x.StudentId == studentId &&
                                x.Id == invitationId &&
                                x.State == InvitationState.WaitingApprove)
                    .FirstOrDefaultAsync();

                if (invitationEntity is null)
                {
                    return TypedResults.Problem("Davetiye bulunamadı!", statusCode: StatusCodes.Status400BadRequest);
                }

                invitationEntity.State = InvitationState.Approved;

                if (invitationEntity.Type == InvitationType.VideoCall)
                {
                    var userId = await zoom.GetUserIdAsync();

                    var topic = $"Baykuş -> {invitationEntity.Coach.Name} & {invitationEntity.Student.Name}";

                    var meetDate = invitationEntity.Date.AddTicks(invitationEntity.StartTime.Ticks);

                    var zoomMeetCreationResponse = await zoom.CreateMeetingAsync(userId, topic, meetDate, 60);

                    var coachMeetUser = await zoom.AddRegistrantAsync(zoomMeetCreationResponse.Id,
                        invitationEntity.Coach.Email,
                        invitationEntity.Coach.CoachDetail.Name, invitationEntity.Coach.CoachDetail.Surname);

                    var studentMeetUser = await zoom.AddRegistrantAsync(zoomMeetCreationResponse.Id,
                        invitationEntity.Student.Email,
                        invitationEntity.Student.StudentDetail.Name, invitationEntity.Student.StudentDetail.Surname);

                    var zoomMeetDetail = new ZoomMeetDetail()
                    {
                        InvitationId = invitationId,
                        MeetId = zoomMeetCreationResponse.Id,
                        HostEmail = zoomMeetCreationResponse.HostEmail,
                        JoinUrl = zoomMeetCreationResponse.JoinUrl,
                        MeetingPasscode = zoomMeetCreationResponse.MeetingPasscode,
                        RegistrationUrl = zoomMeetCreationResponse.RegistrationUrl,
                        StartTime = zoomMeetCreationResponse.StartTime,
                        CreatedAt = zoomMeetCreationResponse.CreatedAt,
                        CoachRegistrantId = coachMeetUser.RegistrantId,
                        CoachParticipantPinCode = coachMeetUser.ParticipantPinCode,
                        StudentRegistrantId = studentMeetUser.RegistrantId,
                        StudentParticipantPinCode = studentMeetUser.ParticipantPinCode,
                        StudentJoinUrl = studentMeetUser.JoinUrl,
                        CoachJoinUrl = coachMeetUser.JoinUrl,
                    };

                    await dbContext.ZoomMeetDetails.AddAsync(zoomMeetDetail);

                    invitationEntity.ZoomMeetDetailId = zoomMeetDetail.Id;
                }

                await dbContext.SaveChangesAsync();

                return TypedResults.Ok();

            }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
            .WithDescription("Öğrenci görüşme saatini onaylar").WithTags(TagConstants.StudentsInvitationApproveOrCancel)
            .RequireAuthorization("Student");
    }
}