using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public static class AssignCoach
{
    public static void MapAssignCoach(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/assign-coach", async Task<Results<Ok, ProblemHttpResult>>
            ([FromBody] AssignCoachRequest assignCoachRequest, ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = sp.GetRequiredService<ApplicationDbContext>();

            var userId = userManager.GetUserId(claimsPrincipal);

            Guid.TryParse(userId, out var id);

            var student = dbContext.Users.Include(x => x.StudentDetail)
                .FirstOrDefault(x => x.Id == id && x.UserType == UserType.Student);

            var coachUser =
                dbContext.Users.FirstOrDefault(x => x.Id == assignCoachRequest.CoachId && x.UserType == UserType.Coach);

            if (coachUser is null || student is null)
            {
                return TypedResults.Problem("Koç atama işlemi başarısız", statusCode: StatusCodes.Status400BadRequest);
            }

            var anyConflict = await dbContext.CoachCalendars.AnyAsync(x => x.CoachId == assignCoachRequest.CoachId &&
                                              x.Date == assignCoachRequest.Date &&
                                              (
                                                  (x.StartTime <= assignCoachRequest.StartTime &&
                                                   x.EndTime > assignCoachRequest
                                                       .StartTime) || // Başlangıç saati, bir kaydın bitiş saatinden önce veya eşitse
                                                  (x.StartTime < assignCoachRequest.EndTime &&
                                                   x.EndTime >=
                                                   assignCoachRequest
                                                       .EndTime) || // Bitiş saati, bir kaydın başlangıç saatinden sonra veya eşitse
                                                  (x.StartTime >= assignCoachRequest.StartTime &&
                                                   x.EndTime <= assignCoachRequest.EndTime)
                                              ));

            if (anyConflict)
            {
                return TypedResults.Problem("", statusCode: StatusCodes.Status400BadRequest);
            }
            
            dbContext.CoachCalendars.Add(new CoachCalendar()
            {
                CoachId = assignCoachRequest.CoachId,
                StudentId = student.Id,
                IsAvailable = false,
                Date = assignCoachRequest.Date,
                StartTime = assignCoachRequest.StartTime,
                EndTime = assignCoachRequest.EndTime,
            });

            student.StudentDetail.TermsAndConditionsAccepted = true;
            student.StudentDetail.Status = StudentStatus.Active;
            
            await dbContext.SaveChangesAsync();
            
            return TypedResults.Ok();
        }).RequireAuthorization().ProducesProblem(StatusCodes.Status404NotFound);
    }

    public class AssignCoachRequest
    {
        public Guid CoachId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}