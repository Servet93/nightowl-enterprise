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
                var paginationUriBuilder = sp.GetRequiredService<PaginationUriBuilder>();
                
                var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                var coachId = claimsPrincipal.GetId();
                
                var user = dbContext.Users
                    .Include(x => x.CoachDetail)
                    .Include(x => x.CoachDetail.University)
                    .FirstOrDefault(x => x.Id == coachId && x.UserType != UserType.Student);

                var rank = dbContext.CoachYksRankings.OrderByDescending(x => Convert.ToInt32(x.Year))
                    .FirstOrDefault(x => x.Enter == true && x.CoachId == coachId).Rank;

                var profilePhotoUrl = paginationUriBuilder.GetCoachProfilePhotoUri(coachId);

                return TypedResults.Ok(CreateInfoResponseAsync(user!, rank, profilePhotoUrl));
            }).RequireAuthorization("Coach").ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
            .WithTags(TagConstants.CoachMeInfo);
    }
    
    private static CoachStateResponse CreateInfoResponseAsync(ApplicationUser user, uint? rank, string? profilePhotoUrl)
    {
        return new CoachStateResponse
        {
            Name = user.CoachDetail.Name,
            Surname = user.CoachDetail.Surname,
            Birthdate = user.CoachDetail.BirthDate,
            UniversityName = user.CoachDetail.University?.Name,
            DepartmentName = user.CoachDetail.DepartmentName,
            YksRank = rank,
            ProfilePhotoUrl = profilePhotoUrl,
            Email = user.Email!,
            Status = user.CoachDetail.Status.Value,
        };
    }

    public class CoachStateResponse
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        
        public string? ProfilePhotoUrl { get; set; }
        
        public DateTime? Birthdate { get; set; }
        
        public string UniversityName { get; set; }
        
        public string DepartmentName { get; set; }
        
        public uint? YksRank { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CoachStatus Status { get; set; }
    }
}