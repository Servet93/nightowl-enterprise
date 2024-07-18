using System.Globalization;
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
                                (x.State == InvitationState.SpecifyHour || x.State == InvitationState.Cancelled))
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
                
                // ILoggerFactory örneği al
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                // ILogger örneğini oluştur
                var logger = loggerFactory.CreateLogger("SpecifyHour");

                invitationEntity.StartTime = specifyHourRequest.StartTime;
                invitationEntity.EndTime = specifyHourRequest.StartTime.Add(TimeSpan.FromHours(1));
                invitationEntity.State = InvitationState.WaitingApprove;

                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    logger.LogError(e,
                        "NotSpecifiedHour. CoachId: {CoachId}, StudentId: {StudentId}, InvitationId: {InvitationId}",
                        coachId, invitationEntity.StudentId, invitationId);
                    
                    var errorDescriptor = new ErrorDescriptor("NotSpecifiedHour", "Saat bilgisi atanamadı!");
                    
                    return errorDescriptor.CreateProblem("Görüşme saati belirlenemedi!");
                }

                try
                {
                    var chatClientService = sp.GetRequiredService<ChatClientService>();

                    var message = string.Empty;
                    var textForSender = string.Empty;
                    var textForReceiver = string.Empty;
                    
                    var cultureInfo = new CultureInfo("tr-TR");
                        
                    if (invitationEntity.Type == InvitationType.VideoCall)
                    {
                        message =
                            $"{invitationEntity.Date.ToString("d MMMM dddd", cultureInfo)} saat {invitationEntity.StartTime.ToString(@"hh\:mm")} için görüntülü görüşme daveti gönderdiniz";
                        textForSender = $"{invitationEntity.Date.ToString("d MMMM dddd", cultureInfo)} saat {invitationEntity.StartTime.ToString(@"hh\:mm")} için görüntülü görüşme daveti gönderdiniz";
                        textForReceiver = $"{invitationEntity.Date.ToString("d MMMM dddd", cultureInfo)} saat {invitationEntity.StartTime.ToString(@"hh\:mm")} için görüntülü görüşme daveti aldınız";
                    }
                    else if (invitationEntity.Type == InvitationType.VoiceCall)
                    {
                        message =
                            $"{invitationEntity.Date.ToString("d MMMM dddd", cultureInfo)} saat {invitationEntity.StartTime.ToString(@"hh\:mm")} için sesli görüşme daveti gönderdiniz";
                        textForSender = $"{invitationEntity.Date.ToString("d MMMM dddd", cultureInfo)} saat {invitationEntity.StartTime.ToString(@"hh\:mm")} için sesli görüşme daveti gönderdiniz";
                        textForReceiver = $"{invitationEntity.Date.ToString("d MMMM dddd", cultureInfo)} saat {invitationEntity.StartTime.ToString(@"hh\:mm")} için sesli görüşme daveti aldınız";
                    }
                    
                    await chatClientService.SendSystemMessage(coachId.ToString(), invitationEntity.StudentId.ToString(), message, new SystemMessage()
                    {
                        Date = invitationEntity.Date,
                        Time = invitationEntity.StartTime,
                        InvitationId = invitationId.ToString(),
                        InvitationType = invitationEntity.Type.ToString(),
                        TextForSender = textForSender,
                        TextForReceiver = textForReceiver,
                        SystemMessageType = SystemMessageType.SpecifiedHour.ToString()
                    });
                }
                catch (Exception e)
                {
                    logger.LogError(e,
                        "CouldNotSendSpecifiedHourSystemMessage. CoachId: {CoachId}, StudentId: {StudentId}, InvitationId: {InvitationId}",
                        coachId, invitationEntity.StudentId, invitationId);
                    
                    var errorDescriptor = new ErrorDescriptor("CouldNotSendSpecifiedHourSystemMessage", "Görüşme saat bilgisi gönderilemedi!");
                    
                    return errorDescriptor.CreateProblem("Görüşme saati bilgisi sohbet ekranına aktarılamadı!");
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