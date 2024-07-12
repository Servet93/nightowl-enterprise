using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using NightOwlEnterprise.Api.Endpoints.CommonDto;
using NightOwlEnterprise.Api.Entities;
using NightOwlEnterprise.Api.Entities.Enums;
using NightOwlEnterprise.Api.Entities.Nets;
using NightOwlEnterprise.Api.Entities.PrivateTutoring;
using Swashbuckle.AspNetCore.Filters;

namespace NightOwlEnterprise.Api.Endpoints.Coachs.Me;

public static class Onboard
{
    private static readonly EmailAddressAttribute EmailAddressAttribute = new();
    
    public static void MapOnboard(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/me/onboard", async Task<Results<Ok, ProblemHttpResult>>
                (CoachOnboardRequest request, ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                var requestValidation = RequestValidation(request);

                if (requestValidation.IsFailure)
                {
                    var errors = requestValidation.Errors;

                    return TypedResults.Problem("Tanışma formu eksik yada hatalı!",
                        extensions: new Dictionary<string, object?>()
                        {
                            { "errors", errors },
                        });
                }

                var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                var strCoachId = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

                Guid.TryParse(strCoachId, out var coachId);

                var coach = dbContext.Users
                    .Include(x => x.CoachDetail)
                    .Include(x => x.CoachYksRankings)
                    .Include(x => x.PrivateTutoringTYT)
                    .Include(x => x.PrivateTutoringTM)
                    .Include(x => x.PrivateTutoringMF)
                    .Include(x => x.PrivateTutoringSozel)
                    .Include(x => x.PrivateTutoringDil)
                    .Include(x => x.TytNets)
                    .Include(x => x.MfNets)
                    .Include(x => x.TmNets)
                    .Include(x => x.DilNets)
                    .Include(x => x.SozelNets)
                    .FirstOrDefault(x => x.Id == coachId && x.UserType == UserType.Coach);

                if (coach.CoachDetail is null)
                {
                    coach.CoachDetail = new CoachDetail();
                }
                
                coach.CoachDetail.Name = request.PersonalInfo.Name;
                coach.CoachDetail.Surname = request.PersonalInfo.Surname;
                coach.CoachDetail.Email = request.PersonalInfo.Email;
                coach.CoachDetail.Mobile = request.PersonalInfo.Mobile;
                coach.CoachDetail.BirthDate = request.PersonalInfo.BirthDate;
                coach.CoachDetail.DepartmentType = request.DepartmentAndExamInfo.DepartmentType;

                coach.CoachDetail.UniversityId = request.DepartmentAndExamInfo.UniversityId;
                coach.CoachDetail.DepartmentName = request.DepartmentAndExamInfo.DepartmentName;

                coach.CoachYksRankings.Clear();

                if (request.DepartmentAndExamInfo.YearToTyt != null)
                {
                    foreach (var yearToTyt in request.DepartmentAndExamInfo.YearToTyt)
                    {
                        uint? _rank = null;
                        
                        if (request.DepartmentAndExamInfo.YearToYksRanking != null && 
                            request.DepartmentAndExamInfo.YearToYksRanking.ContainsKey(yearToTyt.Key))
                        {
                            _rank = request.DepartmentAndExamInfo.YearToYksRanking[yearToTyt.Key];
                        }
                        
                        coach.CoachYksRankings.Add(new CoachYksRanking()
                        {
                            Enter = yearToTyt.Value,
                            Year = yearToTyt.Key.ToString(),
                            Rank = _rank
                        });    
                    }    
                }
                
                coach.CoachDetail.HighSchool = request.AcademicSummary.HighSchool;
                coach.CoachDetail.HighSchoolGPA = request.AcademicSummary.HighSchoolGPA;
                coach.CoachDetail.FirstTytNet = request.AcademicSummary.FirstTytNet;
                coach.CoachDetail.LastTytNet = request.AcademicSummary.LastTytNet;
                coach.CoachDetail.FirstAytNet = request.AcademicSummary.FirstAytNet;
                coach.CoachDetail.LastAytNet = request.AcademicSummary.LastAytNet;
                coach.CoachDetail.ChangedDepartmentType = request.AcademicSummary.ChangedDepartmentType;
                
                if (request.AcademicSummary.ChangedDepartmentType)
                {
                    coach.CoachDetail.FromDepartment = request.AcademicSummary.FromDepartment;
                    coach.CoachDetail.ToDepartment = request.AcademicSummary.ToDepartment;    
                }

                coach.CoachDetail.GoneCramSchool = request.SupplementaryMaterials.Course;
                coach.CoachDetail.UsedYoutube = request.SupplementaryMaterials.Youtube;
                coach.CoachDetail.GoneCramSchool = request.SupplementaryMaterials.School;
                coach.CoachDetail.PrivateTutoring = request.SupplementaryMaterials.PrivateTutoring;

                if (request.SupplementaryMaterials.PrivateTutoring)
                {
                    if (request.SupplementaryMaterials.PrivateTutoringTyt)
                    {
                        if (coach.PrivateTutoringTYT is null)
                        {
                            coach.PrivateTutoringTYT = new global::NightOwlEnterprise.Api.Entities.PrivateTutoring.PrivateTutoringTYT();
                        }
                        
                        coach.PrivateTutoringTYT.Biology =
                            request.SupplementaryMaterials.PrivateTutoringTytLessons.Biology;
                        coach.PrivateTutoringTYT.Chemistry =
                            request.SupplementaryMaterials.PrivateTutoringTytLessons.Chemistry;
                        coach.PrivateTutoringTYT.Geography =
                            request.SupplementaryMaterials.PrivateTutoringTytLessons.Geography;
                        coach.PrivateTutoringTYT.Geometry =
                            request.SupplementaryMaterials.PrivateTutoringTytLessons.Geometry;
                        coach.PrivateTutoringTYT.History =
                            request.SupplementaryMaterials.PrivateTutoringTytLessons.History;
                        coach.PrivateTutoringTYT.Mathematics =
                            request.SupplementaryMaterials.PrivateTutoringTytLessons.Mathematics;
                        coach.PrivateTutoringTYT.Philosophy =
                            request.SupplementaryMaterials.PrivateTutoringTytLessons.Philosophy;
                        coach.PrivateTutoringTYT.Physics =
                            request.SupplementaryMaterials.PrivateTutoringTytLessons.Physics;
                        coach.PrivateTutoringTYT.Religion =
                            request.SupplementaryMaterials.PrivateTutoringTytLessons.Religion;
                        coach.PrivateTutoringTYT.Turkish =
                            request.SupplementaryMaterials.PrivateTutoringTytLessons.Turkish;
                    }
                    
                    if (coach.CoachDetail.DepartmentType == DepartmentType.TM)
                    {
                        if (coach.PrivateTutoringTM is null)
                        {
                            coach.PrivateTutoringTM = new PrivateTutoringTM();
                        }
                        
                        coach.PrivateTutoringTM.Geography =
                            request.SupplementaryMaterials.PrivateTutoringAytLessons.Tm.Geography;
                        coach.PrivateTutoringTM.Geometry =
                            request.SupplementaryMaterials.PrivateTutoringAytLessons.Tm.Geometry;
                        coach.PrivateTutoringTM.History =
                            request.SupplementaryMaterials.PrivateTutoringAytLessons.Tm.History;
                        coach.PrivateTutoringTM.Literature =
                            request.SupplementaryMaterials.PrivateTutoringAytLessons.Tm.Literature;
                        coach.PrivateTutoringTM.Mathematics =
                            request.SupplementaryMaterials.PrivateTutoringAytLessons.Tm.Mathematics;
                        
                    }else if (coach.CoachDetail.DepartmentType == DepartmentType.MF)
                    {
                        if (coach.PrivateTutoringMF is null)
                        {
                            coach.PrivateTutoringMF = new PrivateTutoringMF();
                        }
                        
                        coach.PrivateTutoringMF.Biology =
                            request.SupplementaryMaterials.PrivateTutoringAytLessons.Mf.Biology;
                        coach.PrivateTutoringMF.Chemistry =
                            request.SupplementaryMaterials.PrivateTutoringAytLessons.Mf.Chemistry;
                        coach.PrivateTutoringMF.Geometry =
                            request.SupplementaryMaterials.PrivateTutoringAytLessons.Mf.Geometry;
                        coach.PrivateTutoringMF.Mathematics =
                            request.SupplementaryMaterials.PrivateTutoringAytLessons.Mf.Mathematics;
                        coach.PrivateTutoringMF.Physics =
                            request.SupplementaryMaterials.PrivateTutoringAytLessons.Mf.Physics;
                    }else if (coach.CoachDetail.DepartmentType == DepartmentType.Sozel)
                    {
                        if (coach.PrivateTutoringSozel is null)
                        {
                            coach.PrivateTutoringSozel = new PrivateTutoringSozel();
                        }
                        
                        coach.PrivateTutoringSozel.Geography1 =
                            request.SupplementaryMaterials.PrivateTutoringAytLessons.Sozel.Geography1;
                        coach.PrivateTutoringSozel.Geography2 =
                            request.SupplementaryMaterials.PrivateTutoringAytLessons.Sozel.Geography2;
                        coach.PrivateTutoringSozel.History1 =
                            request.SupplementaryMaterials.PrivateTutoringAytLessons.Sozel.History1;
                        coach.PrivateTutoringSozel.History2 =
                            request.SupplementaryMaterials.PrivateTutoringAytLessons.Sozel.History2;
                        coach.PrivateTutoringSozel.Literature1 =
                            request.SupplementaryMaterials.PrivateTutoringAytLessons.Sozel.Literature1;
                        coach.PrivateTutoringSozel.Philosophy =
                            request.SupplementaryMaterials.PrivateTutoringAytLessons.Sozel.Philosophy;
                        coach.PrivateTutoringSozel.Religion =
                            request.SupplementaryMaterials.PrivateTutoringAytLessons.Sozel.Religion;
                    }else if (coach.CoachDetail.DepartmentType == DepartmentType.Dil)
                    {
                        if (coach.PrivateTutoringDil is null)
                        {
                            coach.PrivateTutoringDil = new PrivateTutoringDil();
                        }

                        coach.PrivateTutoringDil.YTD =
                            request.SupplementaryMaterials.PrivateTutoringAytLessons.Dil.YTD;
                    }
                }

                if (coach.TytNets is null)
                {
                    coach.TytNets = new TYTNets();
                }
                
                coach.TytNets.Biology = request.TytNets.Biology;
                coach.TytNets.Chemistry = request.TytNets.Chemistry;
                coach.TytNets.Geography = request.TytNets.Geography;
                coach.TytNets.Geometry = request.TytNets.Geometry;
                coach.TytNets.History = request.TytNets.History;
                coach.TytNets.Mathematics = request.TytNets.Mathematics;
                coach.TytNets.Philosophy = request.TytNets.Philosophy;
                coach.TytNets.Physics = request.TytNets.Physics;
                coach.TytNets.Religion = request.TytNets.Religion;
                coach.TytNets.Turkish = request.TytNets.Turkish;

                if (request.DepartmentAndExamInfo.DepartmentType == DepartmentType.TM)
                {
                    if (coach.TmNets is null)
                    {
                        coach.TmNets = new TMNets();
                    }
                    
                    coach.TmNets.Geography = request.TmNets.Geography;
                    coach.TmNets.Geometry = request.TmNets.Geometry;
                    coach.TmNets.History = request.TmNets.History;
                    coach.TmNets.Literature = request.TmNets.Literature;
                    coach.TmNets.Mathematics = request.TmNets.Mathematics;
                }else if (request.DepartmentAndExamInfo.DepartmentType == DepartmentType.MF)
                {
                    if (coach.MfNets is null)
                    {
                        coach.MfNets = new MFNets();
                    }
                    
                    coach.MfNets.Biology = request.MfNets.Biology;
                    coach.MfNets.Chemistry = request.MfNets.Chemistry;
                    coach.MfNets.Geometry = request.MfNets.Geometry;
                    coach.MfNets.Mathematics = request.MfNets.Mathematics;
                    coach.MfNets.Physics = request.MfNets.Physics;
                }else if (request.DepartmentAndExamInfo.DepartmentType == DepartmentType.Sozel)
                {
                    if (coach.SozelNets is null)
                    {
                        coach.SozelNets = new global::NightOwlEnterprise.Api.Entities.Nets.SozelNets();
                    }
                    
                    coach.SozelNets.Geography1 = request.SozelNets.Geography1;
                    coach.SozelNets.Geography2 = request.SozelNets.Geography2;
                    coach.SozelNets.History1 = request.SozelNets.History1;
                    coach.SozelNets.History2 = request.SozelNets.History2;
                    coach.SozelNets.Literature1 = request.SozelNets.Literature1;
                    coach.SozelNets.Philosophy = request.SozelNets.Philosophy;
                    coach.SozelNets.Religion = request.SozelNets.Religion;
                }else if (request.DepartmentAndExamInfo.DepartmentType == DepartmentType.Dil)
                {
                    if (coach.DilNets is null)
                    {
                        coach.DilNets = new global::NightOwlEnterprise.Api.Entities.Nets.DilNets();
                    }
                    
                    coach.DilNets.YDT = request.DilNets.YDT;
                }

                coach.CoachDetail.Status = CoachStatus.Active;
                
                await dbContext.SaveChangesAsync();
                
                return TypedResults.Ok();
            }).RequireAuthorization("Coach").Produces<ProblemHttpResult>(400).WithOpenApi()
            .WithTags(TagConstants.CoachMeOnboard);
        
        Result RequestValidation(CoachOnboardRequest request)
        {
            var errorDescriptors = new List<ErrorDescriptor>();
            
            errorDescriptors.AddRange(ValidatePersonalInfo(request.PersonalInfo));
            
            errorDescriptors.AddRange(ValidateDepartmentAndExamInfo(request.DepartmentAndExamInfo));
            
            errorDescriptors.AddRange(ValidateAcademicSummary(request.AcademicSummary));
            
            errorDescriptors.AddRange(ValidateSupplementaryMaterials(request.DepartmentAndExamInfo.DepartmentType, request.SupplementaryMaterials));

            errorDescriptors.AddRange(ValidateLastPracticeTytExamPoints(request.TytNets));
            
            switch (request.DepartmentAndExamInfo.DepartmentType)
            {
                case DepartmentType.TM:
                    errorDescriptors.AddRange(
                        ValidateLastPracticeTmExamPoints(request.TmNets));
                    break;
                case DepartmentType.MF:
                    errorDescriptors.AddRange(
                        ValidateLastPracticeMfExamPoints(request.MfNets));
                    break;
                case DepartmentType.Sozel:
                    errorDescriptors.AddRange(
                        ValidateLastPracticeSozelExamPoints(request.SozelNets));
                    break;
                case DepartmentType.Dil:
                    errorDescriptors.AddRange(
                        ValidateLastPracticeDilExamPoints(request.DilNets));
                    break;
            }

            return errorDescriptors.Any() ? Result.Failure(errorDescriptors) : Result.Success();
        }
    }
    
    private static List<ErrorDescriptor> ValidateDepartmentAndExamInfo(DepartmentAndExamInfo? request)
    {
        var errorDescriptors = new List<ErrorDescriptor>();

        if (request is null)
        {
            errorDescriptors.Add(CommonErrorDescriptor.EmptyAcademicSummary());
        }
        else
        {
            if (string.IsNullOrEmpty(request.DepartmentName))
            {
                errorDescriptors.Add(CommonErrorDescriptor.EmptyDepartmentName());
            }

            var years = request.YearToTyt;

            var yearsDict = new Dictionary<uint, bool>();
            
            var firstYear = 2016;
            
            var lastYear = DateTime.Now.Year;

            for (var i = 0; i < lastYear - firstYear; i++)
            {
                if (years != null && years.ContainsKey((uint)(firstYear + i)))
                {
                    yearsDict.Add((uint)(firstYear + i), years[(uint)(firstYear + i)]);
                }
                else
                {
                    yearsDict.Add((uint)(firstYear + i), false);    
                }
            }

            years = yearsDict;

            if (errorDescriptors.Any())
            {
                return errorDescriptors;
            }

            if (request.YearToYksRanking != null)
            {
                foreach (var yearToYksRanking in request.YearToYksRanking)
                {
                    if (!request.YearToTyt.ContainsKey(yearToYksRanking.Key))
                    {
                        errorDescriptors.Add(CommonErrorDescriptor.InvalidYksRankingYear(yearToYksRanking.Key));
                        continue;
                    }
                
                    if (!request.YearToTyt[yearToYksRanking.Key])
                    {
                        errorDescriptors.Add(CommonErrorDescriptor.NotEnterTytYear(yearToYksRanking.Key));
                        continue;
                    }
                
                    // if (yearToYksRanking.Value > 10000)
                    // {
                    //     errorDescriptors.Add(
                    //         CommonErrorDescriptor.InvalidYksRanking(yearToYksRanking.Key, yearToYksRanking.Value));
                    // }
                }    
            }
        }
        
        return errorDescriptors;
    }

    private static List<ErrorDescriptor> ValidateAcademicSummary(CoachAcademicSummary? request)
    {
        var errorDescriptors = new List<ErrorDescriptor>();

        if (request is null)
        {
            errorDescriptors.Add(CommonErrorDescriptor.EmptyAcademicSummary());
        }
        else
        {
            if (string.IsNullOrEmpty(request.HighSchool))
            {
                errorDescriptors.Add(CommonErrorDescriptor.EmptyHighSchool());
            }

            if (request.HighSchoolGPA is < 0 or > 100)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidHighSchoolGPA(request.HighSchoolGPA));
            }
            
            if (request.FirstTytNet is < 0 or > 120)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidFirstTytNet(request.FirstTytNet));
            }
            
            if (request.LastTytNet is < 0 or > 120)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidLastTytNet(request.LastTytNet));
            }
            
            if (request.FirstAytNet is < 0 or > 80)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidFirstAytNet(request.FirstAytNet));
            }
            
            if (request.LastAytNet is < 0 or > 80)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidLastAytNet(request.LastAytNet));
            }
        }
        
        return errorDescriptors;
    }
    
    private static List<ErrorDescriptor> ValidateSupplementaryMaterials(DepartmentType departmentType, CoachSupplementaryMaterials? request)
    {
        var errorDescriptors = new List<ErrorDescriptor>();
        
        if (request is null)
        {
            errorDescriptors.Add(CommonErrorDescriptor.EmptySupplementaryMaterials());
        }
        else
        {
            if (request.PrivateTutoring)
            {
                if (request.PrivateTutoringAyt)
                {
                    if (request.PrivateTutoringAytLessons is null)
                    {
                        errorDescriptors.Add(CommonErrorDescriptor.EmptyTM());
                    }
                    else
                    {
                        switch (departmentType)
                        {
                            case DepartmentType.TM when request.PrivateTutoringAytLessons?.Tm is null:
                                errorDescriptors.Add(CommonErrorDescriptor.EmptyTM());
                                break;
                            case DepartmentType.MF when request.PrivateTutoringAytLessons?.Mf is null:
                                errorDescriptors.Add(CommonErrorDescriptor.EmptyMF());
                                break;
                            case DepartmentType.Sozel
                                when request.PrivateTutoringAytLessons?.Sozel is null:
                                errorDescriptors.Add(CommonErrorDescriptor.EmptySozel());
                                break;
                            case DepartmentType.Dil when request.PrivateTutoringAytLessons?.Dil is null:
                                errorDescriptors.Add(CommonErrorDescriptor.EmptyDil());
                                break;
                        }
                    }
                }
            }    
        }
        
        return errorDescriptors;
    }

    private static List<ErrorDescriptor> ValidateLastPracticeDilExamPoints(CoachDilNets? request)
    {
        var errorDescriptors = new List<ErrorDescriptor>();
        
        if (request is null)
        {
            errorDescriptors.Add(CommonErrorDescriptor.EmptyLastPracticeYDTExamPoints());
        }
        else
        {
            if (request.YDT > 80)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidYdtNet",
                    "Yabancı dil neti",
                    request.YDT, 80, 0));
            }
        }

        return errorDescriptors;
    }

    private static List<ErrorDescriptor> ValidateLastPracticeSozelExamPoints(CoachSozelNets? request)
    {
        var errorDescriptors = new List<ErrorDescriptor>();
        
        if (request is null)
        {
            errorDescriptors.Add(CommonErrorDescriptor.EmptyLastPracticeMFExamPoints());
        }
        else
        {
            //Tarih-1: (Max 10, Min 0)
            if (request.History1 > 10)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidHistory1Net",
                    "Tarih-1 neti",
                    request.History1, 10, 0));
            }

            //Coğrafya-1: (Max 24, Min 0)
            if (request.Geography1 > 24)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidGeometryNet",
                    "Coğrafya-1 neti",
                    request.Geography1, 24, 0));
            }

            //Edebiyat-1: (Max 6, Min 0)
            if (request.Literature1 > 6)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidLiterature1Net",
                    "Edebiyat-1 neti",
                    request.Literature1, 6, 0));
            }

            //Tarih-2: (Max 11, Min 0)
            if (request.History2 > 11)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidHistory2Net", "Tarih-2 neti",
                    request.History2, 11, 0));
            }

            //Coğrafya-2: (Max 11, Min 0)
            if (request.Geography2 > 11)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidGeography2Net",
                    "Coğrafya-2 neti",
                    request.Geography2, 11, 0));
            }

            //Felsefe: (Max 12, Min 0)
            if (request.Philosophy > 12)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidPhilosophyNet",
                    "Felsefe neti",
                    request.Philosophy, 12, 0));
            }

            //DİN: (Max 6, Min 0)
            if (request.Religion > 6)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidReligionNet",
                    "Din neti",
                    request.Philosophy, 6, 0));
            }
        }

        return errorDescriptors;
    }

    private static List<ErrorDescriptor> ValidateLastPracticeMfExamPoints(CoachMfNets? request)
    {
        var errorDescriptors = new List<ErrorDescriptor>();
        
        if (request is null)
        {
            errorDescriptors.Add(CommonErrorDescriptor.EmptyLastPracticeMFExamPoints());
        }
        else
        {
            //Matematik: (Max 30, Min 0)
            if (request.Mathematics > 30)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidMathematicsNet",
                    "Matematik neti",
                    request.Mathematics, 30, 0));
            }

            //Geometri: (Max 10, Min 0)
            if (request.Geometry > 10)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidGeometryNet",
                    "Geometri neti",
                    request.Geometry, 10, 0));
            }

            //Fizik: (Max 14, Min 0)
            if (request.Physics > 14)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidPhysicsNet",
                    "Fizik neti",
                    request.Physics, 14, 0));
            }

            //Kimya: (Max 13, Min 0)
            if (request.Chemistry > 13)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidChemistryNet", "Kimya neti",
                    request.Chemistry, 13, 0));
            }

            //Biyoloji: (Max 13, Min 0)
            if (request.Biology > 13)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidBiologyNet",
                    "Biyoloji neti",
                    request.Biology, 13, 0));
            }
        }
        
        return errorDescriptors;
    }

    private static List<ErrorDescriptor> ValidateLastPracticeTmExamPoints(CoachTmNets? request)
    {
        var errorDescriptors = new List<ErrorDescriptor>();
        
        if (request is null)
        {
            errorDescriptors.Add(CommonErrorDescriptor.EmptyLastPracticeTMExamPoints());
        }
        else
        {
            //Matematik: (Max 30, Min 0)
            if (request.Mathematics > 30)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidMathematicsNet", "Matematik neti",
                    request.Mathematics, 30, 0));
            }

            //Geometri: (Max 10, Min 0)
            if (request.Geometry > 10)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidGeometryNet", "Geometri neti",
                    request.Geometry, 10, 0));
            }

            //Edebiyat: (Max 24, Min 0)
            if (request.Literature > 24)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidLiteratureNet", "Edebiyat neti",
                    request.Literature, 24, 0));
            }

            //Tarih: (Max 10, Min 0)
            if (request.History > 10)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidHistoryNet", "Tarih neti",
                    request.History, 10, 0));
            }

            //Coğrafya: (Max 6, Min 0)
            if (request.Geography > 6)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidGeographyNet", "Coğrafya neti",
                    request.Geography, 6, 0));
            }
        }
        
        return errorDescriptors;
    }

    private static List<ErrorDescriptor> ValidateLastPracticeTytExamPoints(CoachTytNets? request)
    {
        var errorDescriptors = new List<ErrorDescriptor>();
        
        if (request is null)
        {
            errorDescriptors.Add(CommonErrorDescriptor.EmptyLastPracticeTYTExamPoints());
        }
        else
        {
            //Türkçe Bilgisi: (Max 40, Min 0)
            if (request.Turkish > 40)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidTurkishNet", "Türkçe bilgisi neti",
                    request.Turkish, 40, 0));
            }
            
            //Matematik: (Max 30, Min 0)
            if (request.Mathematics > 30)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidMathematicsNet", "Matematik neti",
                    request.Mathematics, 30, 0));
            }
            
            //Geometri: (Max 10, Min 0)
            if (request.Geometry > 10)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidGeometryNet", "Geometri neti",
                    request.Mathematics, 10, 0));
            }
            
            //Tarih: (Max 5, Min 0)
            if (request.History > 5)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidHistoryNet", "Tarih neti",
                    request.History, 5, 0));
            }
            
            //Coğrafya: (Max 5, Min 0)
            if (request.Geography > 5)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidGeographyNet", "Coğrafya neti",
                    request.Geography, 5, 0));
            }
            
            //Felsefe: (Max 5, Min 0)
            if (request.Philosophy > 5)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidPhilosophyNet", "Felsefe neti",
                    request.Philosophy, 5, 0));
            }
            
            //Din: (Max 5, Min 0)
            if (request.Religion > 5)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidReligionNet", "Din neti",
                    request.Religion, 5, 0));
            }
            
            //Fizik: (Max 7, Min 0)
            if (request.Physics > 7)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidPhysicsNet", "Fizik neti",
                    request.Physics, 7, 0));
            }
            
            //Kimya: (Max 6, Min 0)
            if (request.Chemistry > 6)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidChemistryNet", "Kimya neti",
                    request.Chemistry, 6, 0));
            }
            
            //Biyoloji: (Max 6, Min 0)
            if (request.Biology > 6)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidBiologyNet", "Biyoloji neti",
                    request.Biology, 6, 0));
            }    
        }
        
        return errorDescriptors;
    }

    private static List<ErrorDescriptor> ValidatePersonalInfo(PersonalInfo? request)
    {
        var errorDescriptors = new List<ErrorDescriptor>();

        if (request is null)
        {
            errorDescriptors.Add(CommonErrorDescriptor.EmptyPersonalInfo());
        }
        else
        {
            if (string.IsNullOrEmpty(request.Name) || request.Name.Length < 3)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidPersonalName(request.Name));
            }

            if (string.IsNullOrEmpty(request.Surname) || request.Surname.Length < 2)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidPersonalSurname(request.Surname));
            }

            if (string.IsNullOrEmpty(request.Email) || !EmailAddressAttribute.IsValid(request.Email))
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidPersonalEmail(request.Email));
            }

            if (string.IsNullOrEmpty(request.Mobile))
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidPersonalMobile(request.Mobile));
            }
            else
            {
                //533-222-88-44
                if (string.IsNullOrEmpty(request.Mobile) ||
                    request.Mobile.Length != 13 ||
                    request.Mobile.Split("-").Length != 4 ||
                    request.Mobile.Split("-")[0].First() != '5' ||
                    !int.TryParse(request.Mobile.Split("-")[0], out var parseResult) ||
                    !int.TryParse(request.Mobile.Split("-")[1], out parseResult) ||
                    !int.TryParse(request.Mobile.Split("-")[2], out parseResult) ||
                    !int.TryParse(request.Mobile.Split("-")[3], out parseResult))
                {
                    errorDescriptors.Add(CommonErrorDescriptor.InvalidPersonalMobile(request.Mobile));
                }
            }

            // Bugünün tarihi
            DateTime bugun = DateTime.Today;

            // Yaş hesaplama
            int yas = bugun.Year - request.BirthDate.Year;

            // Doğum günü henüz gelmediyse, yaş bir azaltılır
            if (request.BirthDate > bugun.AddYears(-yas))
            {
                yas--;
            }
            
            if (yas < 21)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidAge(yas));
            }
        }

        return errorDescriptors;
    }

    private static async Task<InfoResponse> CreateInfoResponseAsync<TUser>(TUser user, UserManager<TUser> userManager)
        where TUser : class
    {
        return new()
        {
            Email = await userManager.GetEmailAsync(user) ?? throw new NotSupportedException("Users must have an email."),
            IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user),
        };
    }

    public class OnboardCoach
    {
        public ObjectId Id { get; set; }
        
        public string UserId { get; set; }
        
        public CoachOnboardRequest Data { get; set; }
    }

    /*
     * //Alan -> MF,TM,Sözel,Dil,Tyt
        public string ExamType { get; set; }
        
        //Sınıf -> 9-10-11-12 ve Mezun
        public string Grade  { get; set; }
     */
    


    public class PersonalInfo
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public DateTime BirthDate { get; set; }
    }
    
    //Okuduğunuz lise
    //Lise orta öğretim başarı puanınız 0-100 arasında olmalı
    public class CoachAcademicSummary
    {
        public string HighSchool { get; set; }

        public float HighSchoolGPA { get; set; }

        public byte FirstTytNet { get; set; }
        
        public byte LastTytNet { get; set; }
        
        public byte FirstAytNet { get; set; }
        
        public byte LastAytNet { get; set; }

        public bool ChangedDepartmentType { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DepartmentType? FromDepartment { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DepartmentType? ToDepartment { get; set; }
    };
    
    public class CoachOnboardRequest
    {
        public PersonalInfo? PersonalInfo { get; set; }
        
        public DepartmentAndExamInfo? DepartmentAndExamInfo { get; set; }
        public CoachAcademicSummary? AcademicSummary { get; set; }
        
        public CoachTytNets? TytNets { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise && ExamType -> MF
        public CoachMfNets? MfNets { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise && ExamType -> TM
        public CoachTmNets? TmNets { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise && ExamType -> Sozel
        public CoachSozelNets? SozelNets { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise && ExamType -> Dil
        public CoachDilNets? DilNets { get; set; }
        
        public CoachSupplementaryMaterials? SupplementaryMaterials { get; set; }
    
    }

    public class DepartmentAndExamInfo
    {
        public Guid UniversityId { get; set; }

        public string DepartmentName { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DepartmentType DepartmentType { get; set; }
        
        public Dictionary<uint, bool> YearToTyt { get; set; } 

        public Dictionary<uint, uint> YearToYksRanking { get; set; }
    }
    
    public class CoachSupplementaryMaterials
    {
        public bool School { get; set; }
        //Özel Ders: Bunu işaretlerse hemen altına ek bir soru gelir:
        // Hangi derslerden özel ders alıyorsunuz?(Birden çok seçenek seçebilecek). Seçenekler:
        // TYT(Başlık):
        //Türkçe, Matematik, Geometri, Tarih, Coğrafya, Felsefe, Din, Fizik, Kimya, Biyoloji
        //AYT(Başlık):
        //Alanına göre a partında seçtiği dersler gelir. Hangi alandan hangi derslerin geleceğini e partındaki netler kısmından ulaşabilirsiniz.
        public bool PrivateTutoring { get; set; }
        
        public bool PrivateTutoringTyt { get; set; }
        public CoachPrivateTutoringTYT? PrivateTutoringTytLessons { get; set; }
        
        public bool PrivateTutoringAyt { get; set; }
        //AYT(Başlık):
        //Alanına göre a partında seçtiği dersler gelir. Hangi alandan hangi derslerin geleceğini e partındaki netler kısmından ulaşabilirsiniz.
        public CoachPrivateTutoringAYT? PrivateTutoringAytLessons { get; set; }
        
        public bool Course { get; set; }
        public bool Youtube { get; set; }
    }

    public class CoachPrivateTutoringTYT
    {
        public bool Turkish { get; set; }
        public bool Mathematics { get; set; }
        public bool Geometry { get; set; }
        public bool History { get; set; }
        public bool Geography { get; set; }
        public bool Philosophy { get; set; }
        public bool Religion { get; set; }
        public bool Physics { get; set; }
        public bool Chemistry { get; set; }
        public bool Biology { get; set; }
    }
    
    public class CoachPrivateTutoringAYT
    {
        public CoachMF? Mf { get; set; }
        public CoachTM? Tm { get; set; }
        public CoachSozel? Sozel { get; set; }
        public CoachDil? Dil { get; set; }
    }
    
    public class CoachMF
    {
        public bool Mathematics { get; set; }
        public bool Geometry { get; set; }
        public bool Physics { get; set; }
        public bool Chemistry { get; set; }
        public bool Biology { get; set; }
    }
    
    public class CoachTM
    {
        //Matematik: (Max 30, Min 0)
        public bool Mathematics { get; set; }
        //Geometri: (Max 10, Min 0)
        public bool Geometry { get; set; }
        //Edebiyat: (Max 24, Min 0)
        public bool Literature { get; set; }
        //Tarih: (Max 10, Min 0)
        public bool History { get; set; }
        //Coğrafya: (Max 6, Min 0)
        public bool Geography { get; set; }
    }
    
    //Sözel
    public class CoachSozel
    {
        //Tarih-1: (Max 10, Min 0)
        public bool History1 { get; set; }
        //Coğrafya: (Max 24, Min 0)
        public bool Geography1 { get; set; }
        //Edebiyat-1: (Max 6, Min 0)
        public bool Literature1 { get; set; }
        //Tarih-2: (Max 11, Min 0)
        public bool History2 { get; set; }
        //Coğrafya-2: (Max 11, Min 0)
        public bool Geography2 { get; set; }
        //Felsefe: (Max 12, Min 0)
        public bool Philosophy { get; set; }
        //Din: (Max 6, Min 0)
        public bool Religion { get; set; }
    }
    
    public class CoachDil
    {
        //YDT: (Max 80, Min 0)
        public bool YTD { get; set; }
    }

    public class CoachOnboardRequestExamples : IMultipleExamplesProvider<CoachOnboardRequest>
    {
        private PersonalInfo servetPersonalInfoTYT = new PersonalInfo()
        {
            Name = "Servet",
            Surname = "ŞEKER",
            Email = "servetseker@gmail.com",
            Mobile = "533-333-33-33",
            BirthDate = new DateTime(1993,11,13)
        };

        private DepartmentAndExamInfo departmentAndExamInfo = new DepartmentAndExamInfo()
        {
            UniversityId = Guid.Parse("efd3e340-041d-48a7-824c-b49a282fcb50"),
            DepartmentName = "Bilgisayar Mühendisi",
            DepartmentType = DepartmentType.MF,
            YearToTyt = new Dictionary<uint, bool>()
            {
                { 2016, false },
                { 2017, false },
                { 2018, false },
                { 2019, false },
                { 2020, false },
                { 2021, true },
                { 2022, true },
                { 2023, true },
            },
            YearToYksRanking = new Dictionary<uint, uint>()
            {
                { 2021, 9000 },
                { 2022, 8000 },
                { 2023, 7000 },
            }
        };

        private CoachAcademicSummary _coachAcademicSummary = new CoachAcademicSummary()
        {
            HighSchool = "Güngören Anadolu Teknik",
            HighSchoolGPA = 92.4f,
            ChangedDepartmentType = true,
            FromDepartment = DepartmentType.TM,
            ToDepartment = DepartmentType.MF,
            FirstAytNet = 58,
            LastAytNet = 69,
            FirstTytNet = 73,
            LastTytNet = 87,
        };
        
        private CoachSupplementaryMaterials _coachSupplementaryMaterials = new CoachSupplementaryMaterials()
        {
            Course = true,
            School = true,
            Youtube = false,
            PrivateTutoring = true,
            PrivateTutoringTyt = true,
            PrivateTutoringAyt = true,
            PrivateTutoringTytLessons = new CoachPrivateTutoringTYT()
            {
                Mathematics = true,
                Geometry = true,
                Chemistry = true,
            },
            PrivateTutoringAytLessons = new CoachPrivateTutoringAYT()
            {
                Mf = new CoachMF()
                {
                    Mathematics = true,
                    Geometry = true,
                    Physics = true,
                }
            }
        };
        
        public IEnumerable<SwaggerExample<CoachOnboardRequest>> GetExamples()
        {
            yield return SwaggerExample.Create("Servet,Department:MF", new CoachOnboardRequest()
            {
                PersonalInfo = servetPersonalInfoTYT,
                DepartmentAndExamInfo = departmentAndExamInfo,
                AcademicSummary = _coachAcademicSummary,
                SupplementaryMaterials = _coachSupplementaryMaterials,
                MfNets = new CoachMfNets()
                {
                    Biology = 6,
                    Chemistry = 4,
                    Geometry = 8,
                    Mathematics = 21,
                    Physics = 5,
                },
                TytNets = new CoachTytNets()
                {
                    Biology = 2,
                    Chemistry = 3,
                    Geography = 2,
                    Geometry = 5,
                    History = 1,
                    Mathematics = 24,
                    Philosophy = 2,
                    Physics = 3,
                    Religion = 3,
                    Turkish = 17,
                }
            });
        }
    }
}