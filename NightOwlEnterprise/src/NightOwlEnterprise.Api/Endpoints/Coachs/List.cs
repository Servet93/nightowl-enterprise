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

namespace NightOwlEnterprise.Api.Endpoints.Coachs;

public static class List
{
    public static void MapList(this IEndpointRouteBuilder endpoints)
    {
        // NOTE: We cannot inject UserManager<TUser> directly because the TUser generic parameter is currently unsupported by RDG.
        // https://github.com/dotnet/aspnetcore/issues/47338
        endpoints.MapPost("/", async Task<Results<Ok<PagedResponse<Coach>>, ProblemHttpResult>>
            ([FromBody] CoachFilterRequest? filter, [FromQuery] int? page,[FromQuery] int? pageSize, ClaimsPrincipal claimsPrincipal, HttpContext httpContext, [FromServices] IServiceProvider sp) =>
        {
            var paginationFilter = new PaginationFilter(page, pageSize);
            
            var dbContext = sp.GetRequiredService<ApplicationDbContext>();
            
            var paginationUriBuilder = sp.GetRequiredService<PaginationUriBuilder>();

            var studentId = claimsPrincipal.GetId();

            var student =
                await dbContext.Users
                    .Include(x => x.StudentDetail)
                    .FirstOrDefaultAsync(x => x.Id == studentId && x.UserType == UserType.Student);

            if (student is null)
            {
                return TypedResults.Problem("Öğrenci kayıtlı değil.", statusCode: StatusCodes.Status400BadRequest);
            }

            var studentExamType = student.StudentDetail.ExamType;
            
            var coachAppUsers = new List<ApplicationUser>();
            
            IQueryable<ApplicationUser> coachQueryable = dbContext.Users
                .Include(x => x.CoachDetail)
                .Include(x => x.CoachDetail.University)
                .Include(x => x.CoachYksRankings)
                .Include(x => x.InvitationsAsCoach)
                .Where(x => x.UserType == UserType.Coach)
                .Where(x =>
                    (x.CoachStudentTrainingSchedules.Where(i => i.CoachId == x.Id && i.Day == DayOfWeek.Monday).Count() < x.CoachDetail.MondayQuota) ||
                    (x.CoachStudentTrainingSchedules.Where(i => i.CoachId == x.Id && i.Day == DayOfWeek.Tuesday).Count() < x.CoachDetail.TuesdayQuota) ||
                    (x.CoachStudentTrainingSchedules.Where(i => i.CoachId == x.Id && i.Day == DayOfWeek.Wednesday).Count() < x.CoachDetail.WednesdayQuota) ||
                    (x.CoachStudentTrainingSchedules.Where(i => i.CoachId == x.Id && i.Day == DayOfWeek.Thursday).Count() < x.CoachDetail.ThursdayQuota) ||
                    (x.CoachStudentTrainingSchedules.Where(i => i.CoachId == x.Id && i.Day == DayOfWeek.Friday).Count() < x.CoachDetail.FridayQuota) ||
                    (x.CoachStudentTrainingSchedules.Where(i => i.CoachId == x.Id && i.Day == DayOfWeek.Saturday).Count() < x.CoachDetail.SaturdayQuota) ||
                    (x.CoachStudentTrainingSchedules.Where(i => i.CoachId == x.Id && i.Day == DayOfWeek.Sunday).Count() < x.CoachDetail.SundayQuota)
                );

            if (filter is null)
            {
                // var mongoDatabase = sp.GetRequiredService<IMongoDatabase>();
                //
                // var onboardStudentCollection = mongoDatabase.GetCollection<Students.Onboard.OnboardStudent>("onboardStudents");
                //
                // var docFilter = Builders<Students.Onboard.OnboardStudent>.Filter.Eq(s => s.UserId, strUserId);
                //
                // var onboardStudent = await onboardStudentCollection.Find(docFilter).FirstOrDefaultAsync();
                //
                // if (onboardStudent is null)
                // {
                //     return TypedResults.Problem("Öğrenci kayıtlı değil.", statusCode: StatusCodes.Status400BadRequest);
                // }

                coachQueryable = studentExamType switch
                {
                    ExamType.TM => coachQueryable.Where(x => x.CoachDetail.DepartmentType == DepartmentType.TM),
                    ExamType.MF => coachQueryable.Where(x => x.CoachDetail.DepartmentType == DepartmentType.MF),
                    ExamType.Sozel => coachQueryable.Where(x => x.CoachDetail.DepartmentType == DepartmentType.Sozel),
                    ExamType.Dil => coachQueryable.Where(x => x.CoachDetail.DepartmentType == DepartmentType.Dil),
                    ExamType.TYT => coachQueryable,
                    _ => coachQueryable
                };
            }
            else
            {
                coachQueryable = coachQueryable
                    .WhereIf(x => x.CoachDetail.IsGraduated == filter.IsGraduated, filter.IsGraduated.HasValue)
                    .WhereIf(x => x.CoachDetail.FirstTytNet == filter.FirstTytNet, filter.FirstTytNet.HasValue)
                    .WhereIf(x => x.CoachDetail.GoneCramSchool == filter.GoneCramSchool, filter.GoneCramSchool.HasValue)
                    .WhereIf(x => x.CoachDetail.Male == filter.Male, filter.Male.HasValue)
                    .WhereIf(x => x.CoachDetail.UsedYoutube == filter.UsedYoutube, filter.UsedYoutube.HasValue)
                    .WhereIf(x => x.CoachDetail.Rank > 0 && x.CoachDetail.Rank < 100, filter.Rank.HasValue && filter.Rank == Rank.Between0And100)
                    .WhereIf(x => x.CoachDetail.Rank > 100 && x.CoachDetail.Rank < 1000, filter.Rank.HasValue && filter.Rank == Rank.Between100And1000)
                    .WhereIf(x => x.CoachDetail.Rank > 1000 && x.CoachDetail.Rank < 5000, filter.Rank.HasValue && filter.Rank == Rank.Between1000And5000)
                    .WhereIf(x => x.CoachDetail.Rank > 5000 && x.CoachDetail.Rank < 10000, filter.Rank.HasValue && filter.Rank == Rank.Between5000And10000)
                    .WhereIf(x => x.CoachDetail.Rank >= 10000, filter.Rank.HasValue && filter.Rank == Rank.Between10000And100000)
                    .WhereIf(x => filter.UniversityIds.Contains(x.CoachDetail.UniversityId), filter.UniversityIds is not null && filter.UniversityIds.Any());    
            }

            var totalCount = await coachQueryable.CountAsync();

            coachAppUsers = await coachQueryable.Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize).ToListAsync();
                
            var coachs = new List<Coach>();

            coachs.AddRange(coachAppUsers.Select(x => new Coach()
            {
                Id = x.Id,
                Name = x.Name,
                UniversityName = x.CoachDetail.University.Name,
                DepartmentType = x.CoachDetail.DepartmentType,
                Year = x.CoachYksRankings?.LastOrDefault()?.Year,
                Male = x.CoachDetail.Male,
                Rank = x.CoachDetail.Rank,
                IsGraduated = x.CoachDetail.IsGraduated,
                GoneCramSchool = x.CoachDetail.GoneCramSchool,
                UsedYoutube = x.CoachDetail.UsedYoutube,
                ChangedDepartmentType = x.CoachDetail.ChangedDepartmentType,
                FromDepartment = x.CoachDetail.FromDepartment,
                ToDepartment = x.CoachDetail.ToDepartment,
            }));
                
            var pagedResponse = PagedResponse<Coach>.CreatePagedResponse(
                coachs, totalCount, paginationFilter, paginationUriBuilder,
                httpContext.Request.Path.Value ?? string.Empty);

            return TypedResults.Ok(pagedResponse);
            
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.StudentsCoachListAndReserve).RequireAuthorization("Student");
        
        endpoints.MapGet("/{coachId}", async Task<Results<Ok<Coach>, ProblemHttpResult>>
            ([FromQuery] Guid coachId, ClaimsPrincipal claimsPrincipal, HttpContext httpContext, [FromServices] IServiceProvider sp) =>
        {
            var dbContext = sp.GetRequiredService<ApplicationDbContext>();
            
            var coachApplicationUser = dbContext.Users.Include(x => x.CoachDetail)
                .Include(x => x.CoachDetail.University)
                .FirstOrDefault(x => x.Id == coachId && x.UserType == UserType.Coach);

            if (coachApplicationUser is null)
            {
                return TypedResults.Problem("Koç bilgisi bulunamadı.", statusCode: StatusCodes.Status400BadRequest);
            }

            var coach = new Coach()
            {
                Id = coachApplicationUser.Id,
                Name = coachApplicationUser.Name,
                //Quota = coachApplicationUser.CoachDetail.Quota,
                UniversityName = coachApplicationUser.CoachDetail.University.Name,
                DepartmentType = coachApplicationUser.CoachDetail.DepartmentType,
                Male = coachApplicationUser.CoachDetail.Male,
                Rank = coachApplicationUser.CoachDetail.Rank,
                IsGraduated = coachApplicationUser.CoachDetail.IsGraduated,
                GoneCramSchool = coachApplicationUser.CoachDetail.GoneCramSchool,
                UsedYoutube = coachApplicationUser.CoachDetail.UsedYoutube,
                ChangedDepartmentType = coachApplicationUser.CoachDetail.ChangedDepartmentType,
                FromDepartment = coachApplicationUser.CoachDetail.FromDepartment,
                ToDepartment = coachApplicationUser.CoachDetail.ToDepartment,
            };

            return TypedResults.Ok(coach);
            
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.StudentsCoachListAndReserve).RequireAuthorization("Student");
    }

    public sealed class CoachFilterRequest
    {
        public bool? IsGraduated { get; set; }
        public byte? FirstTytNet { get; set; }
        public bool? GoneCramSchool { get; set; }
    
        public bool? UsedYoutube { get; set; }
        
        public Rank? Rank { get; set; }
        public List<Guid> UniversityIds { get; set; }
        public bool? Male { get; set; }
        //Alan değiştirdi mi
        public bool? ChangedSection { get; set; }
    
        public string? FromSection { get; set; }
    
        public string? ToSection { get; set; }
    }

    public enum Rank
    {
        Between0And100,
        Between100And1000,
        Between1000And5000,
        Between5000And10000,
        Between10000And100000,
    }

    public sealed class Coach
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string UniversityName { get; set; }
        
        public string DepartmentName { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DepartmentType DepartmentType { get; set; }
        
        public bool IsGraduated { get; set; }
    
        public byte FirstTytNet { get; set; }
    
        public bool UsedYoutube { get; set; }
    
        public bool GoneCramSchool { get; set; }
        
        public bool Male { get; set; }
    
        //Alan değiştirdi mi
        public bool ChangedDepartmentType { get; set; }
    
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DepartmentType FromDepartment { get; set; }
    
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DepartmentType ToDepartment { get; set; }
        
        public uint Rank { get; set; }
        
        public string Year { get; set; }
    }

    public class CoachFilterRequestExamples : IMultipleExamplesProvider<CoachFilterRequest>
    {
        public IEnumerable<SwaggerExample<CoachFilterRequest>> GetExamples()
        {
            CoachFilterRequest? nullBody = null;
            
            yield return SwaggerExample.Create("null", nullBody);
            
            yield return SwaggerExample.Create("IsGraduated:true", new CoachFilterRequest()
            {
                IsGraduated = true, 
            });
            
            yield return SwaggerExample.Create("GoneCramSchool:true", new CoachFilterRequest()
            {
                GoneCramSchool = true
            });
        }
    }
}