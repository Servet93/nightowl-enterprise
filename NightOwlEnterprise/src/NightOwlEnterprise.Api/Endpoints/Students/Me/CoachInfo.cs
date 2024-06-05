using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NightOwlEnterprise.Api.Endpoints.Students.Me;

public static class CoachInfo
{
    public static void MapCoachInfo(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/me/coach-info", async Task<Results<Ok<CoachInfoResponse>, ProblemHttpResult>>
                (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                var studentId = claimsPrincipal.GetId();

                var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                var paginationUriBuilder = sp.GetRequiredService<PaginationUriBuilder>();
                
                var coachInfoResponse = dbContext.CoachStudentTrainingSchedules
                    .Include(x => x.Coach.CoachDetail)
                    .OrderByDescending(x => x.CreatedAt)
                    .Where(x => x.StudentId == studentId)
                    .Select(x => new CoachInfoResponse()
                    {
                        Id = x.Coach.Id,
                        Name = x.Coach.CoachDetail.Name,
                        Surname = x.Coach.CoachDetail.Surname,
                        UniversityName = x.Coach.CoachDetail.University.Name,
                    })
                    .FirstOrDefault();
                
                var coachProfilePhotoExist = dbContext.ProfilePhotos.Any(x => x.UserId == coachInfoResponse.Id);

                if (coachProfilePhotoExist)
                {
                    coachInfoResponse.PhotoUrl = paginationUriBuilder.GetCoachProfilePhotoUri(coachInfoResponse.Id);
                }

                return TypedResults.Ok(coachInfoResponse);

            }).RequireAuthorization("Student").ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
            .WithTags(TagConstants.StudentsMeInfo);
    }

    public class CoachInfoResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        
        public string Surname { get; set; }
        
        public string UniversityName { get; set; }
        
        public string PhotoUrl { get; set; }
    }

}