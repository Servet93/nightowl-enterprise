using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NightOwlEnterprise.Api.Entities;
using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Endpoints.Coachs.Me;

public static class ManageInfo
{
    public static void MapManageInfo(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/me/info", async Task<Results<Ok<CoachStateResponse>, ProblemHttpResult>>
                (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                var coachId = claimsPrincipal.GetId();
                
                var user = dbContext.Users
                    .Include(x => x.CoachDetail)
                    .FirstOrDefault(x => x.Id == coachId && x.UserType != UserType.Student);

                return TypedResults.Ok(CreateInfoResponseAsync(user!));
            }).RequireAuthorization("Coach").ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
            .WithTags(TagConstants.CoachMeInfo);
    }
    
    private static CoachStateResponse CreateInfoResponseAsync(ApplicationUser user)
    {
        return new CoachStateResponse
        {
            Email = user.Email!,
            Status = user.CoachDetail.Status,
        };
    }

    public class CoachStateResponse
    {
        public string Email { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CoachStatus Status { get; set; }
    }
}