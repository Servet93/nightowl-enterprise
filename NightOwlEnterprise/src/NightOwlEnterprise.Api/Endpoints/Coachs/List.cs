using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NightOwlEnterprise.Api.Endpoints.CommonDto;
using NightOwlEnterprise.Api.Entities;
using NightOwlEnterprise.Api.Entities.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace NightOwlEnterprise.Api.Endpoints.Coachs;

public static class List
{
    public static void MapList(this IEndpointRouteBuilder endpoints)
    {
        // NOTE: We cannot inject UserManager<TUser> directly because the TUser generic parameter is currently unsupported by RDG.
        // https://github.com/dotnet/aspnetcore/issues/47338
        endpoints.MapPost("/", async Task<Results<Ok<PagedResponse<CoachListItem>>, ProblemHttpResult>>
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

            coachQueryable = studentExamType switch
            {
                ExamType.TYT_TM => coachQueryable.Where(x => x.CoachDetail.DepartmentType == DepartmentType.TM),
                ExamType.TM => coachQueryable.Where(x => x.CoachDetail.DepartmentType == DepartmentType.TM),
                ExamType.TYT_MF => coachQueryable.Where(x => x.CoachDetail.DepartmentType == DepartmentType.MF),
                ExamType.MF => coachQueryable.Where(x => x.CoachDetail.DepartmentType == DepartmentType.MF),
                ExamType.TYT_SOZEL => coachQueryable.Where(x => x.CoachDetail.DepartmentType == DepartmentType.Sozel),
                ExamType.Sozel => coachQueryable.Where(x => x.CoachDetail.DepartmentType == DepartmentType.Sozel),
                ExamType.Dil => coachQueryable.Where(x => x.CoachDetail.DepartmentType == DepartmentType.Dil),
            };
            
            if (filter is not null)
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
                    .WhereIf(x => x.CoachDetail.FromDepartment == filter.FromSection && x.CoachDetail.ToDepartment == filter.ToSection, filter.ChangedSection.HasValue && filter.ChangedSection.Value == true && filter.FromSection.HasValue && filter.ToSection.HasValue)
                    .WhereIf(x => filter.UniversityIds.Contains(x.CoachDetail.UniversityId.Value), filter.UniversityIds is not null && filter.UniversityIds.Any());
            }

            var totalCount = await coachQueryable.CountAsync();

            coachAppUsers = await coachQueryable.Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize).ToListAsync();
                
            var coachs = new List<CoachListItem>();

            foreach (var coachAppUser in coachAppUsers)
            {
                var last = coachAppUser.CoachYksRankings?.Where(x => x.Enter)
                    .OrderByDescending(x => Convert.ToInt32(x.Year)).FirstOrDefault();
                
                var year = last?.Year;
                var rank = last?.Rank;

                coachs.Add(new CoachListItem()
                {
                    Id = coachAppUser.Id,
                    Name = coachAppUser.CoachDetail.Name,
                    Surname = coachAppUser.CoachDetail.Surname,
                    UniversityName = coachAppUser.CoachDetail.University.Name,
                    DepartmentName = coachAppUser.CoachDetail.DepartmentName,
                    Year = year,
                    Rank = rank,
                    ProfilePhotoUrl = paginationUriBuilder.GetCoachProfilePhotoUri(coachAppUser.Id)
                });
            }
                
            var pagedResponse = PagedResponse<CoachListItem>.CreatePagedResponse(
                coachs, totalCount, paginationFilter, paginationUriBuilder,
                httpContext.Request.Path.Value ?? string.Empty);

            return TypedResults.Ok(pagedResponse);
            
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.StudentsCoachListAndReserve).RequireAuthorization("Student");
        
        endpoints.MapGet("/{coachId}", async Task<Results<Ok<CoachItem>, ProblemHttpResult>>
            ([FromRoute] Guid coachId, ClaimsPrincipal claimsPrincipal, HttpContext httpContext, [FromServices] IServiceProvider sp) =>
        {
            var dbContext = sp.GetRequiredService<ApplicationDbContext>();
            
            var coachApplicationUser = dbContext.Users
                .Include(x => x.CoachDetail)
                .Include(x => x.CoachYksRankings)
                .Include(x => x.TytNets)
                .Include(x => x.TmNets)
                .Include(x => x.MfNets)
                .Include(x => x.SozelNets)
                .Include(x => x.DilNets)
                .Include(x => x.PrivateTutoringTYT)
                .Include(x => x.PrivateTutoringTM)
                .Include(x => x.PrivateTutoringMF)
                .Include(x => x.PrivateTutoringDil)
                .Include(x => x.PrivateTutoringSozel)
                .Include(x => x.CoachDetail.University)
                .FirstOrDefault(x => x.Id == coachId && x.UserType == UserType.Coach);

            if (coachApplicationUser is null)
            {
                return TypedResults.Problem("Koç bilgisi bulunamadı.", statusCode: StatusCodes.Status400BadRequest);
            }
            
            var paginationUriBuilder = sp.GetRequiredService<PaginationUriBuilder>();
            
            var coach = new CoachItem()
            {
                Id = coachApplicationUser.Id,
                Name = coachApplicationUser.CoachDetail.Name,
                Surname = coachApplicationUser.CoachDetail.Surname,
                //Quota = coachApplicationUser.CoachDetail.Quota,
                UniversityName = coachApplicationUser.CoachDetail.University.Name,
                DepartmentName = coachApplicationUser.CoachDetail.DepartmentName,
                DepartmentType = coachApplicationUser.CoachDetail.DepartmentType.Value,
                IsGraduated = coachApplicationUser.CoachDetail.IsGraduated ?? false,
                //Yardımcı Kaynaklardan hangilerini kullandınız
                GoneCramSchool = coachApplicationUser.CoachDetail.GoneCramSchool ?? false,
                UsedYoutube = coachApplicationUser.CoachDetail.UsedYoutube ?? false,
                School = coachApplicationUser.CoachDetail.School ?? false,
                PrivateTutoring = coachApplicationUser.CoachDetail.PrivateTutoring ?? false,
                //
                ChangedDepartmentType = coachApplicationUser.CoachDetail.ChangedDepartmentType ?? false,
                FromDepartment = coachApplicationUser.CoachDetail.FromDepartment,
                ToDepartment = coachApplicationUser.CoachDetail.ToDepartment,
                HighschoolName = coachApplicationUser.CoachDetail.HighSchool,
                HighschoolGPA = coachApplicationUser.CoachDetail.HighSchoolGPA ?? 0,
                FirstTytNet = coachApplicationUser.CoachDetail.FirstTytNet ?? 0,
                LastTytNet = coachApplicationUser.CoachDetail.LastTytNet ?? 0,
                FirstAytNet = coachApplicationUser.CoachDetail.FirstAytNet ?? 0,
                LastAytNet = coachApplicationUser.CoachDetail.LastAytNet ?? 0,
                YksRanks = new System.Collections.Generic.Dictionary<string, uint>(),
                ProfilePhotoUrl = paginationUriBuilder.GetCoachProfilePhotoUri(coachId)
            };
            
            if (coachApplicationUser.TytNets is not null)
            {
                coach.TytNets = new CoachTytNets()
                {
                    Biology = coachApplicationUser.TytNets.Biology ?? 0,
                    Chemistry = coachApplicationUser.TytNets.Chemistry ?? 0,
                    Geography = coachApplicationUser.TytNets.Geography ?? 0,
                    Geometry = coachApplicationUser.TytNets.Geometry ?? 0,
                    History = coachApplicationUser.TytNets.History ?? 0,
                    Mathematics = coachApplicationUser.TytNets.Mathematics ?? 0,
                    Philosophy = coachApplicationUser.TytNets.Philosophy ?? 0,
                    Physics = coachApplicationUser.TytNets.Physics ?? 0,
                    Religion = coachApplicationUser.TytNets.Religion ?? 0,
                    Turkish = coachApplicationUser.TytNets.Turkish ?? 0,
                };
            }
            
            if (coachApplicationUser.PrivateTutoringTYT is not null)
            {
                coach.PrivateTutoringTyt = new PrivateTutoringTYTObject()
                {
                    Biology = coachApplicationUser.PrivateTutoringTYT.Biology,
                    Chemistry = coachApplicationUser.PrivateTutoringTYT.Chemistry,
                    Geography = coachApplicationUser.PrivateTutoringTYT.Geography,
                    Geometry = coachApplicationUser.PrivateTutoringTYT.Geometry,
                    History = coachApplicationUser.PrivateTutoringTYT.History,
                    Mathematics = coachApplicationUser.PrivateTutoringTYT.Mathematics,
                    Philosophy = coachApplicationUser.PrivateTutoringTYT.Philosophy,
                    Physics = coachApplicationUser.PrivateTutoringTYT.Physics,
                    Religion = coachApplicationUser.PrivateTutoringTYT.Religion,
                    Turkish = coachApplicationUser.PrivateTutoringTYT.Turkish,
                };
            }
            

            if (coachApplicationUser.CoachDetail.DepartmentType == DepartmentType.TM)
            {
                if (coachApplicationUser.PrivateTutoringTM is not null)
                {
                    coach.PrivateTutoringAyt = new PrivateTutoringAYTObject()
                    {
                        Tm = new TM()
                        {
                            Geography = coachApplicationUser.PrivateTutoringTM.Geography,
                            Geometry = coachApplicationUser.PrivateTutoringTM.Geometry,
                            History = coachApplicationUser.PrivateTutoringTM.History,
                            Literature = coachApplicationUser.PrivateTutoringTM.Literature,
                            Mathematics = coachApplicationUser.PrivateTutoringTM.Mathematics,
                        }
                    };    
                }

                if (coachApplicationUser.TmNets is not null)
                {
                    coach.TmNets = new CoachTmNets()
                    {
                        Geography = coachApplicationUser.TmNets.Geography ?? 0,
                        Geometry = coachApplicationUser.TmNets.Geometry ?? 0,
                        History = coachApplicationUser.TmNets.History ?? 0,
                        Literature = coachApplicationUser.TmNets.Literature ?? 0,
                        Mathematics = coachApplicationUser.TmNets.Mathematics ?? 0,
                    };    
                }
            }
            else if (coachApplicationUser.CoachDetail.DepartmentType == DepartmentType.MF)
            {
                if (coachApplicationUser.PrivateTutoringMF is not null)
                {
                    coach.PrivateTutoringAyt = new PrivateTutoringAYTObject()
                    {
                        Mf = new MF()
                        {
                            Biology = coachApplicationUser.PrivateTutoringMF.Biology,
                            Chemistry = coachApplicationUser.PrivateTutoringMF.Chemistry,
                            Geometry = coachApplicationUser.PrivateTutoringMF.Geometry,
                            Mathematics = coachApplicationUser.PrivateTutoringMF.Mathematics,
                            Physics = coachApplicationUser.PrivateTutoringMF.Physics,
                        }
                    };    
                }
                
                if (coachApplicationUser.MfNets is not null)
                {
                    coach.MfNets = new CoachMfNets()
                    {
                        Biology = coachApplicationUser.MfNets.Biology ?? 0,
                        Chemistry = coachApplicationUser.MfNets.Chemistry ?? 0,
                        Geometry = coachApplicationUser.MfNets.Geometry ?? 0,
                        Mathematics = coachApplicationUser.MfNets.Mathematics ?? 0,
                        Physics = coachApplicationUser.MfNets.Physics ?? 0,
                    };    
                }
            }
            else if (coachApplicationUser.CoachDetail.DepartmentType == DepartmentType.Sozel)
            {
                if (coachApplicationUser.PrivateTutoringSozel is not null)
                {
                    coach.PrivateTutoringAyt = new PrivateTutoringAYTObject()
                    {
                        Sozel = new Sozel()
                        {
                            Geography1 = coachApplicationUser.PrivateTutoringSozel.Geography1,
                            Geography2 = coachApplicationUser.PrivateTutoringSozel.Geography2,
                            History1 = coachApplicationUser.PrivateTutoringSozel.History1,
                            History2 = coachApplicationUser.PrivateTutoringSozel.History2,
                            Literature1 = coachApplicationUser.PrivateTutoringSozel.Literature1,
                            Philosophy = coachApplicationUser.PrivateTutoringSozel.Philosophy,
                            Religion = coachApplicationUser.PrivateTutoringSozel.Religion,
                        }
                    };
                }

                if (coachApplicationUser.SozelNets is not null)
                {
                    coach.SozelNets = new CoachSozelNets()
                    {
                        Geography1 = coachApplicationUser.SozelNets.Geography1 ?? 0,
                        Geography2 = coachApplicationUser.SozelNets.Geography2 ?? 0,
                        History1 = coachApplicationUser.SozelNets.History1 ?? 0,
                        History2 = coachApplicationUser.SozelNets.History2 ?? 0,
                        Literature1 = coachApplicationUser.SozelNets.Literature1 ?? 0,
                        Philosophy = coachApplicationUser.SozelNets.Philosophy ?? 0,
                        Religion = coachApplicationUser.SozelNets.Religion ?? 0,
                    };
                }
                
            }
            else if (coachApplicationUser.CoachDetail.DepartmentType == DepartmentType.Dil)
            {
                if (coachApplicationUser.PrivateTutoringDil is not null)
                {
                    coach.PrivateTutoringAyt = new PrivateTutoringAYTObject()
                    {
                        Dil = new Dil()
                        {
                            YTD = coachApplicationUser.PrivateTutoringDil.YTD,
                        }
                    };
                }

                if (coachApplicationUser.DilNets is not null)
                {
                    coach.DilNets = new CoachDilNets()
                    {
                        YDT = coachApplicationUser.DilNets.YDT ?? 0
                    };
                }
            }

            var coachYksRankings = coachApplicationUser.CoachYksRankings.Where(x => x.Enter).ToList();

            foreach (var yksRank in coachYksRankings)
            {
                coach.YksRanks.Add(yksRank.Year, yksRank.Rank ?? 0);
            }

            if (coach.YksRanks.Any())
            {
                coach.Year = coach.YksRanks.OrderByDescending(x => Convert.ToInt32(x.Key)).FirstOrDefault().Key;
                coach.Rank = coach.YksRanks.OrderByDescending(x => Convert.ToInt32(x.Key)).FirstOrDefault().Value;
            }

            return TypedResults.Ok(coach);
            
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.StudentsCoachListAndReserve).RequireAuthorization("Student");
    }

    public sealed class CoachFilterRequest
    {
        public bool? IsGraduated { get; set; }
        public byte? FirstTytNet { get; set; }
        public bool? GoneCramSchool { get; set; }
    
        public bool? UsedYoutube { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Rank? Rank { get; set; }
        public List<Guid> UniversityIds { get; set; }
        public bool? Male { get; set; }
        //Alan değiştirdi mi
        public bool? ChangedSection { get; set; }
    
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DepartmentType? FromSection { get; set; }
    
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DepartmentType? ToSection { get; set; }
    }

    public enum Rank
    {
        Between0And100,
        Between100And1000,
        Between1000And5000,
        Between5000And10000,
        Between10000And100000,
    }

    public sealed class CoachListItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UniversityName { get; set; }
        public string DepartmentName { get; set; }
        
        public string ProfilePhotoUrl { get; set; }
        public uint? Rank { get; set; }
        public string Year { get; set; }
    }
    
    public sealed class CoachItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UniversityName { get; set; }
        public string DepartmentName { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DepartmentType DepartmentType { get; set; }
        
        //İlk ve son tyt-ayt netleri        
        public byte FirstTytNet { get; set; }
        
        public byte LastTytNet { get; set; }
        
        public byte FirstAytNet { get; set; }
        
        public byte LastAytNet { get; set; }
        
        //
        public bool IsGraduated { get; set; }
        
        public string HighschoolName { get; set; }
        
        public float HighschoolGPA { get; set; }
        
    
        //Alan değiştirdi mi
        public bool ChangedDepartmentType { get; set; }
    
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DepartmentType? FromDepartment { get; set; }
    
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DepartmentType? ToDepartment { get; set; }
        
        public uint? Rank { get; set; }
        public string Year { get; set; }

        public Dictionary<string, uint> YksRanks { get; set; }
        
        //Nets

        public CoachTytNets TytNets { get; set; }
        
        public CoachMfNets MfNets { get; set; }
        
        public CoachTmNets TmNets { get; set; }
        
        public CoachSozelNets SozelNets { get; set; }
        
        public CoachDilNets DilNets { get; set; }
        
        //Yardımcı Kaynaklardan hangilerini kullandınız
        public bool UsedYoutube { get; set; }
        public bool GoneCramSchool { get; set; }
        public bool PrivateTutoring { get; set; }
        public bool School { get; set; }
        
        //Özel ders 
        public PrivateTutoringTYTObject PrivateTutoringTyt { get; set; }
        
        public PrivateTutoringAYTObject PrivateTutoringAyt { get; set; }
        
        public string ProfilePhotoUrl { get; set; }
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