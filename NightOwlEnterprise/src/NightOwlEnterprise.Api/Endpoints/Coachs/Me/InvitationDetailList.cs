﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using NightOwlEnterprise.Api.Entities.Enums;
using Stripe;
using Swashbuckle.AspNetCore.Annotations;

namespace NightOwlEnterprise.Api.Endpoints.Coachs.Me;

public static class InvitationDetailList
{
    public static void MapInvitationDetailList(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/me/todo-list", async Task<Results<Ok<List<InvitationResponse>>, ProblemHttpResult>>
                (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                var coachId = claimsPrincipal.GetId();

                var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                var openStates = new InvitationState[2]
                {
                    InvitationState.SpecifyHour, InvitationState.WaitingApprove,
                };

                var now = DateTime.UtcNow.ConvertUtcToTimeZone();
                var nextThreeDays = DateTime.UtcNow.ConvertUtcToTimeZone().AddDays(2);
                
                var invitationEntities = await dbContext.Invitations
                    .Include(x => x.Student)
                    .Include(x => x.Student.StudentDetail)
                    .Include(x => x.ZoomMeetDetail)
                    // .Where(x => x.CoachId == coachId && x.Date >= now && x.Date <= nextThreeDays)
                    .Where(x => x.CoachId == coachId)
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                var invitations = invitationEntities.Select(invitationEntity => new InvitationResponse()
                    {
                        Id = invitationEntity.Id,
                        StudentId = invitationEntity.StudentId,
                        StudentName = invitationEntity.Student.StudentDetail.Name,
                        StudentSurname = invitationEntity.Student.StudentDetail.Surname,
                        Type = invitationEntity.Type,
                        State = invitationEntity.State,
                        Date = invitationEntity.Date,
                        StartTime = invitationEntity.StartTime,
                        EndTime = invitationEntity.EndTime,
                        JoinUrl = invitationEntity.ZoomMeetDetail?.CoachJoinUrl,
                        Enabled = invitationEntity.Date == now && (now.TimeOfDay - invitationEntity.StartTime).Minutes < 5
                    })
                    .ToList();

                return TypedResults.Ok(invitations);

            }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
            .WithDescription("Koç davetiyelerini çeker.")
            .WithTags(TagConstants.CoachScheduling).RequireAuthorization("Coach");

        endpoints.MapGet("/me/invitations/{invitationId}",
                async Task<Results<Ok<InvitationResponse>, ProblemHttpResult>>
                ([FromRoute] Guid invitationId, ClaimsPrincipal claimsPrincipal,
                    [FromServices] IServiceProvider sp) =>
                {
                    var coachId = claimsPrincipal.GetId();

                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                    var invitationEntity = await dbContext.Invitations
                        .Include(x => x.Student)
                        .Include(x => x.Student.StudentDetail)
                        .Include(x => x.ZoomMeetDetail)
                        .Where(x => x.CoachId == coachId && x.Id == invitationId)
                        .FirstOrDefaultAsync();

                    var now = DateTime.UtcNow.ConvertUtcToTimeZone();
                    
                    var invitation = new InvitationResponse()
                    {
                        Id = invitationEntity.Id,
                        StudentId = invitationEntity.StudentId,
                        StudentName = invitationEntity.Student.StudentDetail.Name,
                        StudentSurname = invitationEntity.Student.StudentDetail.Surname,
                        Type = invitationEntity.Type,
                        State = invitationEntity.State,
                        Date = invitationEntity.Date,
                        StartTime = invitationEntity.StartTime,
                        EndTime = invitationEntity.EndTime,
                        JoinUrl = invitationEntity.ZoomMeetDetail?.CoachJoinUrl,
                        Enabled = invitationEntity.Date == now && (now.TimeOfDay - invitationEntity.StartTime).Minutes < 5
                    };

                    return TypedResults.Ok(invitation);

                }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
            .WithDescription("Koç belirtilen davetiyeyi çeker.").WithTags(TagConstants.CoachScheduling)
            .RequireAuthorization("Coach");
    }
    
    public sealed class InvitationResponse
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public Guid StudentId { get; set; }
        
        public string StudentName { get; set; }
        
        public string StudentSurname { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public InvitationType Type { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public InvitationState State { get; set; }
        
        public string JoinUrl { get; set; }
        
        public bool Enabled { get; set; }
        
    }
}