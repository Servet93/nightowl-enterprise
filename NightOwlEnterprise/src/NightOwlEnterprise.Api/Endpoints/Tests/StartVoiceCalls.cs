using Hangfire;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Endpoints.Common;

public static class StartVoiceCalls
{
    public static void MapStartVoiceCalls(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/start-voice-calls-from-invitations", async Task<Results<Ok, ProblemHttpResult>>
            ([FromQuery] bool now, [FromServices] IServiceProvider sp) =>
        {
            var dbContext = sp.GetRequiredService<ApplicationDbContext>();
            var bgJobClient = sp.GetRequiredService<IBackgroundJobClient>();

            var t = DateTime.UtcNow.ConvertUtcToTimeZone();
            
            var invitations = await dbContext.Invitations
                .Include(x => x.Student)
                .Include(x => x.Student.StudentDetail)
                .Include(x => x.Coach)
                .Include(x => x.Coach.CoachDetail)
                .Where(x => x.Type == InvitationType.VoiceCall && x.StartTime != TimeSpan.Zero)
                .OrderBy(x => x.Date)
                .Select(x => new
                {
                    InvitationId = x.Id,
                    StudentId = x.StudentId,
                    StudentFullName = x.Student.StudentDetail.Name + x.Student.StudentDetail.Surname,
                    StudentMobile = x.Student.StudentDetail.Mobile,
                    CoachId = x.CoachId,
                    CoachFullName = x.Coach.CoachDetail.Name + x.Coach.CoachDetail.Surname,
                    CoachMobile = x.Coach.CoachDetail.Mobile,
                    Date = x.Date,
                    StartTime = x.StartTime
                }).ToListAsync();

            foreach (var invitation in invitations)
            {
                var scheduledTime = invitation.Date.Add(invitation.StartTime).AddMinutes(-1);

                if (now)
                {
                    bgJobClient.Enqueue<VerimorService>(x => x.Call(invitation.InvitationId));
                }
                else
                {
                    bgJobClient.Schedule<VerimorService>(x => x.Call(invitation.InvitationId), scheduledTime);    
                }
            }
            
            return TypedResults.Ok();
        }).ProducesValidationProblem(401).ProducesValidationProblem(403);
        
         endpoints.MapPost("/start-voice-calls", async Task<Results<Ok, ProblemHttpResult>>
            ([FromBody] StartVoiceCallsRequest request, [FromServices] IServiceProvider sp) =>
        {
            var dbContext = sp.GetRequiredService<ApplicationDbContext>();
            var bgJobClient = sp.GetRequiredService<IBackgroundJobClient>();

            var t = DateTime.UtcNow.ConvertUtcToTimeZone();

            var callList = request.SourceAndDestinations;
            
            foreach (var callItem in callList)
            {
                if (request.Now)
                {
                    bgJobClient.Enqueue<VerimorService>(x => x.CallTest(callItem.Source, callItem.Destination, callItem.Pair));
                }
                else
                {
                    bgJobClient.Schedule<VerimorService>(x => x.CallTest(callItem.Source, callItem.Destination, callItem.Pair),
                        request.Date.Value);
                }
            }
            
            return TypedResults.Ok();
        }).ProducesValidationProblem(401).ProducesValidationProblem(403);
    }
    
    public sealed class StartVoiceCallsRequest
    {
        public bool Now { get; set; }
        public DateTime? Date { get; set; }
        public List<SourceAndDestination> SourceAndDestinations { get; set; }
    }

    public sealed class SourceAndDestination
    {
        public string Pair { get; set; }
        public string Source { get; set; }
        
        public string Destination { get; set; }
    }
}