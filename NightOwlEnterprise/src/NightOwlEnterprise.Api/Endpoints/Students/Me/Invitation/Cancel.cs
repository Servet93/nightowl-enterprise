using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Endpoints.Students.Me.Invitation;

public static class Cancel
{
    public static void MapCancel(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/me/invitations/{invitationId}/cancel", async Task<Results<Ok, ProblemHttpResult>>
            ([FromRoute] Guid invitationId, [FromBody] InvitationCancelRequest request, ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
        {
            var studentId = claimsPrincipal.GetId();
            
            var dbContext = sp.GetRequiredService<ApplicationDbContext>();

            if (string.IsNullOrEmpty(request.Excuse))
            {
                var errorDescriptor = new ErrorDescriptor("ExcuseCouldNotBeEmpty", "Mazeretinizi giriniz!");

                return errorDescriptor.CreateProblem("Davet iptal edilemedi!");
            }
            
            var invitationEntity = await dbContext.Invitations.Include(x => x.Student)
                .Where(x => x.StudentId == studentId &&
                            x.Id == invitationId &&
                            x.State == InvitationState.WaitingApprove)
                .FirstOrDefaultAsync();

            if (invitationEntity is null)
            {
                var errorDescriptor = new ErrorDescriptor("InvitationNotFound", "Davet bulunamadı!");

                return errorDescriptor.CreateProblem("Davet iptal edilemedi!");
            }
            
            // ILoggerFactory örneği al
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            // ILogger örneğini oluştur
            var logger = loggerFactory.CreateLogger("Approve");

            invitationEntity.State = InvitationState.Cancelled;
            invitationEntity.Excuse = request.Excuse;
            
            try
            { 
                await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e,
                    "InvitationCouldNotBeCancelled. CoachId: {CoachId}, StudentId: {StudentId}, InvitationId: {InvitationId}",
                    invitationEntity.CoachId, invitationEntity.StudentId, invitationId);
                    
                var errDesc = new ErrorDescriptor("InvitationCouldNotBeCancelled", "Davet iptal edilemedi!");

                return errDesc.CreateProblem("Davet iptal edilemedi!");
            }

            try
            {
                var chatClientService = sp.GetRequiredService<ChatClientService>();

                var message = string.Empty;
                
                var textForSender = string.Empty;
                var textForReceiver = string.Empty;
                
                if (invitationEntity.Type == InvitationType.VideoCall)
                {
                    message = "Görüntülü görüşme reddedildi.";
                    textForSender = "Görüntülü görüşmeyi reddettiniz.";
                    textForReceiver = "Görüntülü görüşme reddedildi.";
                }
                else if (invitationEntity.Type == InvitationType.VoiceCall)
                {
                    message = "Sesli görüşme reddedildi.";
                    textForSender = "Sesli görüşmeyi reddettiniz.";
                    textForReceiver = "Sesli görüşme reddedildi.";
                }
                    
                await chatClientService.SendSystemMessage(invitationEntity.StudentId.ToString(), invitationEntity.CoachId.ToString(), message, new SystemMessage()
                {
                    Date = invitationEntity.Date,
                    Time = invitationEntity.StartTime,
                    InvitationId = invitationId.ToString(),
                    InvitationType = invitationEntity.Type.ToString(),
                    Excuse = request.Excuse,
                    TextForSender = textForSender,
                    TextForReceiver = textForReceiver,
                    SystemMessageType = SystemMessageType.Cancelled.ToString()
                });
            }
            catch (Exception e)
            {
                logger.LogError(e,
                    "CouldNotBeSentInvitationCancelledSystemMessage. CoachId: {CoachId}, StudentId: {StudentId}, InvitationId: {InvitationId}",
                    invitationEntity.CoachId, invitationEntity.StudentId, invitationId);

                var errDesc = new ErrorDescriptor("CouldNotBeSentInvitationCancelledSystemMessage",
                    "Davet iptal edildiğine dair sistem mesajı kullanıcılara gönderilemedi!");

                return errDesc.CreateProblem("Davet iptal edildi sistem mesajı gönderilemedi!");
            }
            
            return TypedResults.Ok();
            
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithDescription("Öğrenci görüşme saatini reddeder").WithTags(TagConstants.StudentsInvitationApproveOrCancel).RequireAuthorization("Student");
    }

    public class InvitationCancelRequest
    {
        public string Excuse { get; set; }
    }
}