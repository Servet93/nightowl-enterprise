using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public static class CoachInfo
{
    public static void MapCoachInfo(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/coach-info", async Task<Results<Ok<CoachInfoResponse>, ProblemHttpResult>>
            (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
        {
            var studentId = claimsPrincipal.GetId();

            var dbContext = sp.GetRequiredService<ApplicationDbContext>();

            var coachInfoResponse = dbContext.CoachStudentTrainingSchedules
                .Include(x => x.Coach.CoachDetail)
                .OrderByDescending(x => x.CreatedAt)
                .Where(x => x.StudentId == studentId)
                .Select(x => new CoachInfoResponse()
                {
                    Name = x.Coach.CoachDetail.Name,
                    Surname = x.Coach.CoachDetail.Surname,
                    UniversityName = x.Coach.CoachDetail.University.Name
                })
                .FirstOrDefault();

            return TypedResults.Ok(coachInfoResponse);
            
        }).RequireAuthorization("Student").ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags("Öğrenci");
    }

    public class CoachInfoResponse
    {
        public string Name { get; set; }
        
        public string Surname { get; set; }
        
        public string UniversityName { get; set; }
    }

}