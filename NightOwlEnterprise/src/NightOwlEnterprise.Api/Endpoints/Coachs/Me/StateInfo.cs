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

public static class StateInfo
{
    public static void MapStateInfo(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/me/state-info", async Task<Results<Ok<CoachStateResponse>, ProblemHttpResult>>
                (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                var paginationUriBuilder = sp.GetRequiredService<PaginationUriBuilder>();
                
                var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                var coachId = claimsPrincipal.GetId();
                
                var user = dbContext.Users
                    .Include(x => x.CoachDetail)
                    .FirstOrDefault(x => x.Id == coachId && x.UserType != UserType.Student);

                return TypedResults.Ok(CreateStateInfoResponseAsync(user!));
            }).RequireAuthorization("CoachOrPdr").ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
            .WithTags(TagConstants.CoachMeInfo);
    }
    
    private static CoachStateResponse CreateStateInfoResponseAsync(ApplicationUser user)
    {
        return new CoachStateResponse
        {
            Id = user.Id,
            Email = user.Email!,
            Status = user.CoachDetail.Status.Value,
        };
    }

    public class CoachStateResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CoachStatus Status { get; set; }
    }
}