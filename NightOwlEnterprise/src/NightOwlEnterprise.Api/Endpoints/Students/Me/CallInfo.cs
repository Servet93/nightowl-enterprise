using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NightOwlEnterprise.Api.Endpoints.Students.Me;

public static class CallInfo
{
    public static void MapCallInfo(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/me/call-info", async Task<Results<Ok<CallInfoResponse>, ProblemHttpResult>>
                (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                var studentId = claimsPrincipal.GetId();

                var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                var invitations = dbContext.Invitations
                    .Include(x => x.ZoomMeetDetail)
                    .Where(x => x.StudentId == studentId)
                    .OrderByDescending(x => x.Date)
                    .Take(2).Select(x => new
                    {
                        Id = x.Id,
                        Date = x.Date,
                        State = x.State,
                        Type = x.Type,
                        StartTime = x.StartTime,
                        EndTime = x.EndTime,
                        Enabled = x.State == InvitationState.Open,
                        JoinUrl = x.ZoomMeetDetail.StudentJoinUrl,
                    }).ToList();

                var callInfoResponse = new CallInfoResponse();

                var videoCall = invitations.FirstOrDefault(x => x.Type == InvitationType.VideoCall);
                var voiceCall = invitations.FirstOrDefault(x => x.Type == InvitationType.VoiceCall);

                if (videoCall is not null)
                {
                    callInfoResponse.VideoCall = new VideoCall()
                    {
                        Id = videoCall.Id,
                        Date = videoCall.Date,
                        StartTime = videoCall.StartTime,
                        State = videoCall.State,
                        Enabled = videoCall.Enabled,
                        JoinUrl = videoCall.JoinUrl
                    };
                }

                if (voiceCall is not null)
                {
                    callInfoResponse.VoiceCall = new VoiceCall()
                    {
                        Id = voiceCall.Id,
                        Date = voiceCall.Date,
                        StartTime = voiceCall.StartTime,
                        State = voiceCall.State,
                        Enabled = voiceCall.Enabled
                    };
                }

                return TypedResults.Ok(callInfoResponse);
            }).RequireAuthorization("Student").ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
            .WithTags(TagConstants.StudentsMeInfo);
    }

    public class CallInfoResponse
    {
        public VideoCall VideoCall { get; set; }
        
        public VoiceCall VoiceCall { get; set; }
    }

    public class VideoCall
    {
        public Guid Id { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public  InvitationState State { get; set; }

        public bool Enabled { get; set; }
        
        public string JoinUrl { get; set; }
    }
    
    public class VoiceCall
    {
        public Guid Id { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public  InvitationState State { get; set; }

        public bool Enabled { get; set; }
    }
    
}