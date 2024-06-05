using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NightOwlEnterprise.Api.Entities;
using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Endpoints.Students.Me;

public static class StateInfo
{
    public static void MapStateInfo(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/me/state-info", async Task<Results<Ok<StudentStateInfoResponse>, ProblemHttpResult>>
                (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                var userId = userManager.GetUserId(claimsPrincipal);

                Guid.TryParse(userId, out var id);

                var user = dbContext.Users
                    .Include(x => x.SubscriptionHistories)
                    .Include(x => x.StudentDetail)
                    .FirstOrDefault(x => x.Id == id);

                return TypedResults.Ok(CreateInfoResponseAsync(user!));
            }).RequireAuthorization("Student").ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
            .WithTags(TagConstants.StudentsMeInfo);
    }
    
    private static StudentStateInfoResponse CreateInfoResponseAsync(ApplicationUser user)
    {
        var subscription = user.SubscriptionHistories.FirstOrDefault(x =>
            x.SubscriptionEndDate != null && x.SubscriptionEndDate.Value > DateTime.UtcNow);

        return new StudentStateInfoResponse
        {
            Id = user.Id,
            Email = user.Email!,
            Status = user.StudentDetail.Status,
            SubscriptionType = subscription!.Type,
            SubscriptionStartDate = subscription.SubscriptionStartDate,
            SubscriptionEndDate = subscription.SubscriptionEndDate
        };
    }

    public class StudentStateInfoResponse
    {
        public Guid Id { get; set; }
        
        public string Email { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StudentStatus Status { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SubscriptionType SubscriptionType { get; set; }
        public DateTime SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
    }
}