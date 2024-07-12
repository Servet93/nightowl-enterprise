using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Hangfire;
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
                var bgJobClient = sp.GetRequiredService<IBackgroundJobClient>();
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

                try
                {
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
                            invitationEntity.Student.StudentDetail.Name,
                            invitationEntity.Student.StudentDetail.Surname);

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
                    else if (invitationEntity.Type == InvitationType.VoiceCall)
                    {
                        var scheduledTime = invitationEntity.Date.Add(invitationEntity.StartTime).AddMinutes(-1);

                        bgJobClient.Schedule<VerimorService>((v) => v.Call(invitationId), scheduledTime);
                    }

                    await dbContext.SaveChangesAsync();
                    
                    var chatClientService = sp.GetRequiredService<ChatClientService>();

                    var message = string.Empty;
                    var textForSender = string.Empty;
                    var textForReceiver = string.Empty;
                    
                    if (invitationEntity.Type == InvitationType.VideoCall)
                    {
                        message = "Görüntülü görüşme kabul edildi.";
                        textForSender = "Görüntülü görüşmeyi kabul ettiniz.";
                        textForReceiver = "Görüntülü görüşme kabul edildi.";
                    }
                    else if (invitationEntity.Type == InvitationType.VoiceCall)
                    {
                        message = "Sesli görüşme kabul edildi.";
                        textForSender = "Sesli görüşmeyi kabul ettiniz.";
                        textForReceiver = "Sesli görüşme kabul edildi.";
                    }
                    
                    chatClientService.SendInvitationApprovedMessage(invitationEntity.StudentId.ToString(), invitationEntity.CoachId.ToString(), message, new InvitationApprovedMessage()
                    {
                        Date = invitationEntity.Date,
                        Time = invitationEntity.StartTime,
                        InvitationId = invitationId.ToString(),
                        InvitationType = invitationEntity.Type.ToString(),
                        TextForSender =  textForSender,
                        TextForReceiver = textForReceiver,
                        SystemMessageType = SystemMessageType.Approved.ToString()
                    });
                }
                catch (Exception e)
                {
                    var errDesc = new ErrorDescriptor("InvitationNotApproved", "Davet onaylanamadı!");

                    return errDesc.CreateProblem("Davet Onaylanamadı!");
                }

                return TypedResults.Ok();

            }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
            .WithDescription("Öğrenci görüşme saatini onaylar").WithTags(TagConstants.StudentsInvitationApproveOrCancel)
            .RequireAuthorization("Student");
    }
}