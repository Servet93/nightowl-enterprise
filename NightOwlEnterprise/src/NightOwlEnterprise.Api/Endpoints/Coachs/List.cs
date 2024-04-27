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
            
            var strUserId = claimsPrincipal?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            Guid.TryParse(strUserId, out var userId);
            
            var coachAppUsers = new List<ApplicationUser>();

            var now = DateTime.Now;

            var weekDays = now.GetWeekDays();

            IQueryable<ApplicationUser> coachQueryable = dbContext.Users
                .Include(x => x.CoachDetail)
                .Include(x => x.CoachDetail.University)
                .Include(x => x.CoachDetail.Department)
                .Include(x => x.InvitationsAsCoach)
                .Where(x => x.UserType == UserType.Coach)
                .Where(x =>
                    (x.InvitationsAsCoach.Where(i => i.Date == weekDays[0]).Count() < x.CoachDetail.SundayQuota) ||
                    (x.InvitationsAsCoach.Where(i => i.Date == weekDays[1]).Count() < x.CoachDetail.MondayQuota) ||
                    (x.InvitationsAsCoach.Where(i => i.Date == weekDays[2]).Count() < x.CoachDetail.TuesdayQuota) ||
                    (x.InvitationsAsCoach.Where(i => i.Date == weekDays[3]).Count() < x.CoachDetail.WednesdayQuota) ||
                    (x.InvitationsAsCoach.Where(i => i.Date == weekDays[4]).Count() < x.CoachDetail.ThursdayQuota) ||
                    (x.InvitationsAsCoach.Where(i => i.Date == weekDays[5]).Count() < x.CoachDetail.FridayQuota) ||
                    (x.InvitationsAsCoach.Where(i => i.Date == weekDays[6]).Count() < x.CoachDetail.SaturdayQuota));

            if (filter is null)
            {
                var mongoDatabase = sp.GetRequiredService<IMongoDatabase>();

                var onboardStudentCollection = mongoDatabase.GetCollection<Students.Onboard.OnboardStudent>("onboardStudents");

                var docFilter = Builders<Students.Onboard.OnboardStudent>.Filter.Eq(s => s.UserId, strUserId);

                var onboardStudent = await onboardStudentCollection.Find(docFilter).FirstOrDefaultAsync();

                if (onboardStudent is null)
                {
                    return TypedResults.Problem("Öğrenci kayıtlı değil.", statusCode: StatusCodes.Status400BadRequest);
                }

                coachQueryable = onboardStudent.Data.StudentGeneralInfo.ExamType switch
                {
                    Students.Onboard.ExamType.TM => coachQueryable.Where(x => x.CoachDetail.Department.DepartmentType == DepartmentType.TM),
                    Students.Onboard.ExamType.MF => coachQueryable.Where(x => x.CoachDetail.Department.DepartmentType == DepartmentType.MF),
                    Students.Onboard.ExamType.Sozel => coachQueryable.Where(x => x.CoachDetail.Department.DepartmentType == DepartmentType.Sozel),
                    Students.Onboard.ExamType.Dil => coachQueryable.Where(x => x.CoachDetail.Department.DepartmentType == DepartmentType.Dil),
                    Students.Onboard.ExamType.TYT => coachQueryable,
                    _ => coachQueryable
                };
            }
            else
            {
                coachQueryable = coachQueryable
                    .WhereIf(x => x.CoachDetail.IsGraduated == filter.IsGraduated.Value, filter.IsGraduated.HasValue)
                    .WhereIf(x => x.CoachDetail.FirstTytNet == filter.FirstTytNet.Value, filter.FirstTytNet.HasValue)
                    .WhereIf(x => x.CoachDetail.GoneCramSchool == filter.GoneCramSchool, filter.GoneCramSchool.HasValue)
                    .WhereIf(x => x.CoachDetail.Male == filter.Male, filter.Male.HasValue)
                    .WhereIf(x => x.CoachDetail.IsGraduated == filter.IsGraduated, filter.IsGraduated.HasValue)
                    .WhereIf(x => x.CoachDetail.UniversityId == filter.UniversityId.Value, filter.UniversityId.HasValue);    
            }

            var totalCount = await coachQueryable.CountAsync();

            coachAppUsers = await coachQueryable.Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize).ToListAsync();
                
            var coachs = new List<Coach>();

            coachs.AddRange(coachAppUsers.Select(x => new Coach()
            {
                Id = x.Id,
                Name = x.Name,
                //Quota = x.CoachDetail.StudentQuota,
                UniversityName = x.CoachDetail.University.Name,
                DepartmentName = x.CoachDetail.Department.Name,
                DepartmentType = x.CoachDetail.Department.DepartmentType,
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
            
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags("Öğrencinin Koç ile yapabileceği işlemler").RequireAuthorization("Student");
        
        endpoints.MapGet("/{coachId}", async Task<Results<Ok<Coach>, ProblemHttpResult>>
            ([FromQuery] Guid coachId, ClaimsPrincipal claimsPrincipal, HttpContext httpContext, [FromServices] IServiceProvider sp) =>
        {
            var dbContext = sp.GetRequiredService<ApplicationDbContext>();
            
            var coachApplicationUser = dbContext.Users.Include(x => x.CoachDetail)
                .Include(x => x.CoachDetail.University)
                .Include(x => x.CoachDetail.Department)
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
                DepartmentName = coachApplicationUser.CoachDetail.Department.Name,
                DepartmentType = coachApplicationUser.CoachDetail.Department.DepartmentType,
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
            
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags("Öğrencinin Koç ile yapabileceği işlemler").RequireAuthorization("Student");
    }

    public sealed class CoachFilterRequest
    {
        public bool? IsGraduated { get; set; }
        public byte? FirstTytNet { get; set; }
        public bool? GoneCramSchool { get; set; }
    
        public bool? UsedYoutube { get; set; }
        
        public uint? Rank { get; set; }
        public Guid? UniversityId { get; set; }
        public bool? Male { get; set; }
        //Alan değiştirdi mi
        public bool? ChangedSection { get; set; }
    
        public string? FromSection { get; set; }
    
        public string? ToSection { get; set; }

        public byte? Quota { get; set; }
    }

    public sealed class Coach
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        
        //public int Quota { get; set; }

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