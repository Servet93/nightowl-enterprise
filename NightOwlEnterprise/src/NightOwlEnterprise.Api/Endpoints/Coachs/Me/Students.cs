using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using NightOwlEnterprise.Api.Endpoints.Students;
using Stripe;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace NightOwlEnterprise.Api.Endpoints.Coachs.Me;

public static class Students
{
    public static void MapStudents(this IEndpointRouteBuilder endpoints)
    {
        // NOTE: We cannot inject UserManager<TUser> directly because the TUser generic parameter is currently unsupported by RDG.
        // https://github.com/dotnet/aspnetcore/issues/47338
        // endpoints.MapPost("/me/students", async Task<Results<Ok<PagedResponse<StudentItem>>, ProblemHttpResult>>
        //     ([FromQuery] int? page,[FromQuery] int? pageSize, ClaimsPrincipal claimsPrincipal, HttpContext httpContext, [FromServices] IServiceProvider sp) =>
        // {
        //     var paginationFilter = new PaginationFilter(page, pageSize);
        //     
        //     var dbContext = sp.GetRequiredService<ApplicationDbContext>();
        //     
        //     var paginationUriBuilder = sp.GetRequiredService<PaginationUriBuilder>();
        //
        //     var coachId = claimsPrincipal.GetId();
        //     
        //     // dbContext.CoachStudentTrainingSchedules.Where(x => x.CoachId == coachId).Select(x => new StudentItem()
        //     // {
        //     //     Id = x.StudentId,
        //     //     Name = x.Student.Name,
        //     //     
        //     // })
        //
        //     return TypedResults.Ok(new List<StudentItem>() { new StudentItem() });
        //
        // }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags("Öğrencinin Koç ile yapabileceği işlemler").RequireAuthorization("Coach");
        
        // endpoints.MapGet("me/students/{studentId}", async Task<Results<Ok<StudentItem>, ProblemHttpResult>>
        //     ([FromQuery] Guid studentId, ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
        // {
        //     var dbContext = sp.GetRequiredService<ApplicationDbContext>();
        //     
        //     var coachId = claimsPrincipal.GetId();
        //     
        //     var coachApplicationUser = dbContext.Users.Include(x => x.CoachDetail)
        //         .Include(x => x.CoachDetail.University)
        //         .Include(x => x.CoachDetail.Department)
        //         .FirstOrDefault(x => x.Id == coachId && x.UserType == UserType.Coach);
        //
        //     if (coachApplicationUser is null)
        //     {
        //         return TypedResults.Problem("Koç bilgisi bulunamadı.", statusCode: StatusCodes.Status400BadRequest);
        //     }
        //
        //     return TypedResults.Ok(new StudentItem());
        //     
        // }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags("Öğrencinin Koç ile yapabileceği işlemler").RequireAuthorization("Coach");
    }

    public sealed class StudentItem
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public string Surname { get; set; }
        
        public string Highschool { get; set; }
        
        public string DepartmentType { get; set; }
    
        public string Grade { get; set; }
    }
}