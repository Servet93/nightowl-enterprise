using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
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
            
            IQueryable<ApplicationUser> coachQueryable = dbContext.Coachs.Include(x => x.CoachDetail)
                .Include(x => x.CoachDetail.University)
                .Include(x => x.CoachDetail.Department)
                .Where(x => x.CoachDetail.Quota != 0);

            if (filter is null)
            {
                var mongoDatabase = sp.GetRequiredService<IMongoDatabase>();

                var onboardStudentCollection = mongoDatabase.GetCollection<Onboard.OnboardStudent>("onboardStudents");

                var docFilter = Builders<Onboard.OnboardStudent>.Filter.Eq(s => s.UserId, strUserId);

                var onboardStudent = await onboardStudentCollection.Find(docFilter).FirstOrDefaultAsync();

                if (onboardStudent is null)
                {
                    return TypedResults.Problem("Öğrenci kayıtlı değil.", statusCode: StatusCodes.Status400BadRequest);
                }

                coachQueryable = onboardStudent.Data.StudentGeneralInfo.ExamType switch
                {
                    Onboard.ExamType.TM => coachQueryable.Where(x => x.CoachDetail.Tm == true),
                    Onboard.ExamType.MF => coachQueryable.Where(x => x.CoachDetail.Mf == true),
                    Onboard.ExamType.Sozel => coachQueryable.Where(x => x.CoachDetail.Sozel == true),
                    Onboard.ExamType.Dil => coachQueryable.Where(x => x.CoachDetail.Sozel == true),
                    Onboard.ExamType.TYT => coachQueryable.Where(x => x.CoachDetail.Sozel == true),
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
                    .WhereIf(x => x.CoachDetail.UniversityId == filter.UniversityId.Value, filter.UniversityId.HasValue)
                    .WhereIf(x => x.CoachDetail.DepartmentId == filter.DepartmentId.Value, filter.DepartmentId.HasValue);    
            }

            var totalCount = await coachQueryable.CountAsync();

            coachAppUsers = await coachQueryable.Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize).ToListAsync();
                
            var coachs = new List<Coach>();

            coachs.AddRange(coachAppUsers.Select(x => new Coach()
            {
                Id = x.Id,
                Name = x.Name,
                Quota = x.CoachDetail.Quota,
                UniversityName = x.CoachDetail.University.Name,
                DepartmentName = x.CoachDetail.Department.Name,
                Dil = x.CoachDetail.Dil,
                Mf = x.CoachDetail.Mf,
                Tm = x.CoachDetail.Tm,
                Sozel = x.CoachDetail.Sozel,
                Tyt = x.CoachDetail.Tyt,
                Male = x.CoachDetail.Male,
                Rank = x.CoachDetail.Rank,
                IsGraduated = x.CoachDetail.IsGraduated,
                GoneCramSchool = x.CoachDetail.GoneCramSchool,
                UsedYoutube = x.CoachDetail.UsedYoutube,
                ChangedSection = x.CoachDetail.ChangedSection,
                FromSection = x.CoachDetail.FromSection,
                ToSection = x.CoachDetail.ToSection,
            }));
                
            var pagedResponse = PagedResponse<Coach>.CreatePagedResponse(
                coachs, totalCount, paginationFilter, paginationUriBuilder,
                httpContext.Request.Path.Value ?? string.Empty);

            return TypedResults.Ok(pagedResponse);
            
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithTags("Koç");
        
        endpoints.MapGet("/{coachId}", async Task<Results<Ok<Coach>, ProblemHttpResult>>
            ([FromQuery] Guid coachId, ClaimsPrincipal claimsPrincipal, HttpContext httpContext, [FromServices] IServiceProvider sp) =>
        {
            var dbContext = sp.GetRequiredService<ApplicationDbContext>();
            
            var coachApplicationUser = dbContext.Coachs.Include(x => x.CoachDetail)
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
                Quota = coachApplicationUser.CoachDetail.Quota,
                UniversityName = coachApplicationUser.CoachDetail.University.Name,
                DepartmentName = coachApplicationUser.CoachDetail.Department.Name,
                Dil = coachApplicationUser.CoachDetail.Dil,
                Mf = coachApplicationUser.CoachDetail.Mf,
                Tm = coachApplicationUser.CoachDetail.Tm,
                Sozel = coachApplicationUser.CoachDetail.Sozel,
                Tyt = coachApplicationUser.CoachDetail.Tyt,
                Male = coachApplicationUser.CoachDetail.Male,
                Rank = coachApplicationUser.CoachDetail.Rank,
                IsGraduated = coachApplicationUser.CoachDetail.IsGraduated,
                GoneCramSchool = coachApplicationUser.CoachDetail.GoneCramSchool,
                UsedYoutube = coachApplicationUser.CoachDetail.UsedYoutube,
                ChangedSection = coachApplicationUser.CoachDetail.ChangedSection,
                FromSection = coachApplicationUser.CoachDetail.FromSection,
                ToSection = coachApplicationUser.CoachDetail.ToSection,
            };

            return TypedResults.Ok(coach);
            
        }).ProducesProblem(StatusCodes.Status400BadRequest);
    }

    public sealed class CoachFilterRequest
    {
        public bool? IsGraduated { get; set; }
    
        public byte? FirstTytNet { get; set; }
        
        public bool? GoneCramSchool { get; set; }
    
        public bool? UsedYoutube { get; set; }
        
        public uint? Rank { get; set; }

        public Guid? UniversityId { get; set; }
    
        public Guid? DepartmentId { get; set; }
    
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
        
        public int Quota { get; set; }

        public string UniversityName { get; set; }
        
        public string DepartmentName { get; set; }
        
        public bool Tm { get; set; }
        public bool Mf { get; set; }
        public bool Sozel { get; set; }
        public bool Dil { get; set; }
        public bool Tyt { get; set; }

        public bool IsGraduated { get; set; }
    
        public byte FirstTytNet { get; set; }
    
        public bool UsedYoutube { get; set; }
    
        public bool GoneCramSchool { get; set; }
        
        public bool Male { get; set; }
    
        //Alan değiştirdi mi
        public bool ChangedSection { get; set; }
    
        public string? FromSection { get; set; }
    
        public string? ToSection { get; set; }
        
        public uint Rank { get; set; }
    }
}