﻿using System.Security.Claims;
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
                    .WhereIf(x => filter.UniversityIds.Contains(x.CoachDetail.UniversityId.Value), filter.UniversityIds is not null && filter.UniversityIds.Any());
            }

            var totalCount = await coachQueryable.CountAsync();

            coachAppUsers = await coachQueryable.Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize).ToListAsync();
                
            var coachs = new List<CoachListItem>();

            coachs.AddRange(coachAppUsers.Select(x => new CoachListItem()
            {
                Id = x.Id,
                Name = x.CoachDetail.Name,
                Surname = x.CoachDetail.Surname,
                UniversityName = x.CoachDetail.University.Name,
                DepartmentName = x.CoachDetail.DepartmentName,
                Year = x.CoachYksRankings?.LastOrDefault()?.Year,
                Rank = x.CoachDetail.Rank.Value,
            }));
                
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
            
            var coach = new CoachItem()
            {
                Id = coachApplicationUser.Id,
                Name = coachApplicationUser.CoachDetail.Name,
                Surname = coachApplicationUser.CoachDetail.Surname,
                //Quota = coachApplicationUser.CoachDetail.Quota,
                UniversityName = coachApplicationUser.CoachDetail.University.Name,
                DepartmentName = coachApplicationUser.CoachDetail.DepartmentName,
                DepartmentType = coachApplicationUser.CoachDetail.DepartmentType.Value,
                IsGraduated = coachApplicationUser.CoachDetail.IsGraduated.Value,
                //Yardımcı Kaynaklardan hangilerini kullandınız
                GoneCramSchool = coachApplicationUser.CoachDetail.GoneCramSchool.Value,
                UsedYoutube = coachApplicationUser.CoachDetail.UsedYoutube.Value,
                School = coachApplicationUser.CoachDetail.School.Value,
                PrivateTutoring = coachApplicationUser.CoachDetail.PrivateTutoring.Value,
                //
                ChangedDepartmentType = coachApplicationUser.CoachDetail.ChangedDepartmentType.Value,
                FromDepartment = coachApplicationUser.CoachDetail.FromDepartment.Value,
                ToDepartment = coachApplicationUser.CoachDetail.ToDepartment.Value,
                HighschoolName = coachApplicationUser.CoachDetail.HighSchool,
                HighschoolGPA = coachApplicationUser.CoachDetail.HighSchoolGPA.Value,
                FirstTytNet = coachApplicationUser.CoachDetail.FirstTytNet.Value,
                LastTytNet = coachApplicationUser.CoachDetail.LastTytNet.Value,
                FirstAytNet = coachApplicationUser.CoachDetail.FirstAytNet.Value,
                LastAytNet = coachApplicationUser.CoachDetail.FirstAytNet.Value,
                YksRanks = new System.Collections.Generic.Dictionary<string, uint>(),
            };
            
            if (coachApplicationUser.TytNets is not null)
            {
                coach.TytNets = new CoachTytNets()
                {
                    Biology = coachApplicationUser.TytNets.Biology.Value,
                    Chemistry = coachApplicationUser.TytNets.Chemistry.Value,
                    Geography = coachApplicationUser.TytNets.Geography.Value,
                    Geometry = coachApplicationUser.TytNets.Geometry.Value,
                    History = coachApplicationUser.TytNets.History.Value,
                    Mathematics = coachApplicationUser.TytNets.Mathematics.Value,
                    Philosophy = coachApplicationUser.TytNets.Philosophy.Value,
                    Physics = coachApplicationUser.TytNets.Physics.Value,
                    Religion = coachApplicationUser.TytNets.Religion.Value,
                    Grammar = coachApplicationUser.TytNets.Grammar.Value,
                    Semantics = coachApplicationUser.TytNets.Semantics.Value,
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
                        Geography = coachApplicationUser.TmNets.Geography.Value,
                        Geometry = coachApplicationUser.TmNets.Geometry.Value,
                        History = coachApplicationUser.TmNets.History.Value,
                        Literature = coachApplicationUser.TmNets.Literature.Value,
                        Mathematics = coachApplicationUser.TmNets.Mathematics.Value,
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
                        Biology = coachApplicationUser.MfNets.Biology.Value,
                        Chemistry = coachApplicationUser.MfNets.Chemistry.Value,
                        Geometry = coachApplicationUser.MfNets.Geometry.Value,
                        Mathematics = coachApplicationUser.MfNets.Mathematics.Value,
                        Physics = coachApplicationUser.MfNets.Physics.Value,
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
                        Geography1 = coachApplicationUser.SozelNets.Geography1.Value,
                        Geography2 = coachApplicationUser.SozelNets.Geography2.Value,
                        History1 = coachApplicationUser.SozelNets.History1.Value,
                        History2 = coachApplicationUser.SozelNets.History2.Value,
                        Literature1 = coachApplicationUser.SozelNets.Literature1.Value,
                        Philosophy = coachApplicationUser.SozelNets.Philosophy.Value,
                        Religion = coachApplicationUser.SozelNets.Religion.Value,
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
                        YDT = coachApplicationUser.DilNets.YDT.Value
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

    public sealed class CoachListItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UniversityName { get; set; }
        public string DepartmentName { get; set; }
        public uint Rank { get; set; }
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
        public DepartmentType FromDepartment { get; set; }
    
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DepartmentType ToDepartment { get; set; }
        
        public uint Rank { get; set; }
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