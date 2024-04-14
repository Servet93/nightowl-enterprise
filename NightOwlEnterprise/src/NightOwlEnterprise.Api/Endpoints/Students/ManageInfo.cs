using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public static class ManageInfo
{
    public static void MapManageInfo<TUser>(this IEndpointRouteBuilder endpoints)
        where TUser : class, new()
    {
        //var accountGroup = endpoints.MapGroup("/manage").RequireAuthorization();

        endpoints.MapGet("/info", async Task<Results<Ok<InfoResponse>, ProblemHttpResult>>
            (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = sp.GetRequiredService<ApplicationDbContext>();

            var userId = userManager.GetUserId(claimsPrincipal);

            Guid.TryParse(userId, out var id);

            var user = dbContext.Users.Include(x => x.SubscriptionHistories).FirstOrDefault(x => x.Id == id);
            
            // if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
            // {
            //     return TypedResults.Problem(statusCode: StatusCodes.Status404NotFound);
            // }

            return TypedResults.Ok(CreateInfoResponseAsync(user!));
        }).RequireAuthorization().ProducesProblem(StatusCodes.Status404NotFound);
    }
    
    private static InfoResponse CreateInfoResponseAsync(ApplicationUser user)
    {
        var subscription = user.SubscriptionHistories.FirstOrDefault(x =>
            x.SubscriptionEndDate != null && x.SubscriptionEndDate.Value > DateTime.UtcNow);

        return new InfoResponse
        {
            Email = user.Email!,
            Status = user.StudentDetail.Status,
            SubscriptionType = subscription!.Type,
            SubscriptionStartDate = subscription.SubscriptionStartDate,
            SubscriptionEndDate = subscription.SubscriptionEndDate
        };
    }

    public class InfoResponse
    {
        public string Email { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StudentStatus Status { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SubscriptionType SubscriptionType { get; set; }
        public DateTime SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
    }
}