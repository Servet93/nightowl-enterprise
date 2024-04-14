using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public static class Onboard
{
    private static readonly EmailAddressAttribute EmailAddressAttribute = new();
    
    private static string[] examTypes = new string[5] { "mf", "tm", "sozel", "dil", "tyt" };
        
    private static string[] gradeTypes = new string[5] { "9", "10", "11", "12", "mezun" };
    
    public static void MapOnboard(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/onboard/student-general-info", Results<Ok, ProblemHttpResult>
                (StudentGeneralInfo request, [FromServices] IServiceProvider sp) =>
            {
                var requestValidation = ValidateStudentGeneralInfo(request);

                if (requestValidation.Any())
                {
                    return requestValidation.CreateProblem("Öğrenci bilgisi doğrulanamadı");
                }

                return TypedResults.Ok();

            }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
            .WithTags("Öğrenci Tanışma Formu İşlemleri");

        endpoints.MapPost("/onboard/parent-info", Results<Ok, ProblemHttpResult>
                (ParentInfo request, [FromServices] IServiceProvider sp) =>
            {
                var requestValidation = ValidateParentInfo(request);

                if (requestValidation.Any())
                {
                    return requestValidation.CreateProblem("Veli bilgisi doğrulanamadı");
                }

                return TypedResults.Ok();

            }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
            .WithTags("Öğrenci Tanışma Formu İşlemleri");

        endpoints.MapPost("/onboard/last-practice-tyt-exam-points", Results<Ok, ProblemHttpResult>
                (TYTAveragePoints request, [FromServices] IServiceProvider sp) =>
            {
                var requestValidation = ValidateLastPracticeTytExamPoints(request);

                if (requestValidation.Any())
                {
                    return requestValidation.CreateProblem("Son temel yeterlilik sınav türü bilgisi doğrulanamadı");
                }

                return TypedResults.Ok();

            }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
            .WithTags("Öğrenci Tanışma Formu İşlemleri");

        endpoints.MapPost("/onboard/last-practice-tm-exam-points", Results<Ok, ProblemHttpResult>
                (TMAveragePoints request, [FromServices] IServiceProvider sp) =>
            {
                var requestValidation = ValidateLastPracticeTmExamPoints(request);

                if (requestValidation.Any())
                {
                    return requestValidation.CreateProblem("Son eşit ağırlık puan türü bilgisi doğrulanamadı");
                }

                return TypedResults.Ok();

            }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
            .WithTags("Öğrenci Tanışma Formu İşlemleri");

        endpoints.MapPost("/onboard/last-practice-mf-exam-points", Results<Ok, ProblemHttpResult>
                (MFAveragePoints request, [FromServices] IServiceProvider sp) =>
            {
                var requestValidation = ValidateLastPracticeMfExamPoints(request);

                if (requestValidation.Any())
                {
                    return requestValidation.CreateProblem("Son sayısal puan türü bilgisi doğrulanamadı");
                }

                return TypedResults.Ok();

            }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
            .WithTags("Öğrenci Tanışma Formu İşlemleri");

        endpoints.MapPost("/onboard/last-practice-sozel-exam-points", Results<Ok, ProblemHttpResult>
                (SozelAveragePoints request, [FromServices] IServiceProvider sp) =>
            {
                var requestValidation = ValidateLastPracticeSozelExamPoints(request);

                if (requestValidation.Any())
                {
                    return requestValidation.CreateProblem("Son sözel puan türü bilgisi doğrulanamadı");
                }

                return TypedResults.Ok();

            }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
            .WithTags("Öğrenci Tanışma Formu İşlemleri");

        endpoints.MapPost("/onboard/last-practice-dil-exam-points", Results<Ok, ProblemHttpResult>
                (DilAveragePoints request, [FromServices] IServiceProvider sp) =>
            {
                var requestValidation = ValidateLastPracticeDilExamPoints(request);

                if (requestValidation.Any())
                {
                    return requestValidation.CreateProblem("Son yabancı dil puan türü bilgisi doğrulanamadı");
                }

                return TypedResults.Ok();

            }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
            .WithTags("Öğrenci Tanışma Formu İşlemleri");

        endpoints.MapPost("/onboard/student-goals", Results<Ok, ProblemHttpResult>
                (StudentGoals request, [FromServices] IServiceProvider sp) =>
            {
                var requestValidation = ValidateStudentGoals(request);

                if (requestValidation.Any())
                {
                    return requestValidation.CreateProblem("Öğrenci hedefleri doğrulanamadı.");
                }

                return TypedResults.Ok();

            }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
            .WithTags("Öğrenci Tanışma Formu İşlemleri");

        endpoints.MapPost("/onboard/academic-summary", Results<Ok, ProblemHttpResult>
                (AcademicSummary request, [FromServices] IServiceProvider sp) =>
            {
                var requestValidation = ValidateAcademicSummary(request);

                if (requestValidation.Any())
                {
                    return requestValidation.CreateProblem("Öğrenci akademik özet bilgisi doğrulanamadı.");
                }

                return TypedResults.Ok();

            }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
            .WithTags("Öğrenci Tanışma Formu İşlemleri");

        endpoints.MapPost("/onboard/suplementary-materials/{examType}", Results<Ok, ProblemHttpResult>
            ([FromQuery] ExamType examType, [FromBody] SupplementaryMaterials request,
                [FromServices] IServiceProvider sp) =>
            {
                var requestValidation = ValidateSupplementaryMaterials(examType, request);

                if (requestValidation.Any())
                {
                    return requestValidation.CreateProblem("Öğrenci akademik özet bilgisi doğrulanamadı.");
                }

                return TypedResults.Ok();

            }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
            .WithTags("Öğrenci Tanışma Formu İşlemleri");

        endpoints.MapPost("/onboard", async Task<Results<Ok, ProblemHttpResult>>
                (StudentOnboardRequest request, ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
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

                var mongoDatabase = sp.GetRequiredService<IMongoDatabase>();

                var onboardStudentCollection = mongoDatabase.GetCollection<OnboardStudent>("onboardStudents");

                var strUserId = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

                var filter = Builders<OnboardStudent>.Filter.Eq(s => s.UserId, strUserId);

                var onboardStudent = await onboardStudentCollection.Find(filter).FirstOrDefaultAsync();

                if (onboardStudent is null)
                {
                    await onboardStudentCollection.InsertOneAsync(new OnboardStudent()
                    {
                        UserId = strUserId,
                        Data = request,
                    });

                    Guid.TryParse(strUserId, out var userId);

                    var user = await dbContext.Users
                        .Include(x => x.StudentDetail)
                        .Include(x => x.SubscriptionHistories)
                        .FirstOrDefaultAsync(x => x.Id == userId);

                    if (user is not null)
                    {
                        var subscription = user.SubscriptionHistories.Where(x => x.SubscriptionEndDate != null)
                            .OrderBy(x => x.SubscriptionEndDate.Value)
                            .FirstOrDefault(x => x.SubscriptionEndDate.Value > DateTime.UtcNow);
                        
                        if (subscription.Type == SubscriptionType.OnlyPdr)
                        {
                            user!.StudentDetail.Status = StudentStatus.OnboardCompleted;    
                        }else if (subscription.Type == SubscriptionType.PdrWithCoach)
                        {
                            user!.StudentDetail.Status = StudentStatus.CoachSelect;
                        }
                        
                        await dbContext.SaveChangesAsync();
                    }
                }
                else
                {
                    filter = Builders<OnboardStudent>.Filter.Eq(s => s.Id, onboardStudent.Id);

                    await onboardStudentCollection.ReplaceOneAsync(filter, new OnboardStudent()
                    {
                        Id = onboardStudent.Id,
                        UserId = strUserId,
                        Data = request,
                    });
                }

                return TypedResults.Ok();
            }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
            .WithTags("Öğrenci Tanışma Formu İşlemleri");
        
        endpoints.MapPost("/onboard/terms-and-conditions-accepted", async Task<Results<Ok, ProblemHttpResult>>
                (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                var dbContext = sp.GetRequiredService<ApplicationDbContext>();
                
                var strUserId = claimsPrincipal?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

                Guid.TryParse(strUserId, out var userId);

                var user = dbContext.Users
                    .Include(x => x.StudentDetail)
                    // .Include(x => x.SubscriptionHistories)
                    .FirstOrDefault(x => x.Id == userId && x.UserType == UserType.Student);

                // var subscription = user!.SubscriptionHistories.OrderBy(x => x.SubscriptionEndDate)
                //     .FirstOrDefault(
                //         x => x.SubscriptionEndDate.HasValue && x.SubscriptionEndDate.Value > DateTime.UtcNow);
                
                // if (user.StudentDetail.Status == StudentStatus.OnboardCompleted && subscription is not null && subscription.Type == SubscriptionType.OnlyPdr)

                if (user is not null && user.StudentDetail.Status != StudentStatus.OnboardCompleted)
                {
                    return TypedResults.Problem("Öğrenci tanışma formunu tamamlayın",
                        statusCode: StatusCodes.Status400BadRequest);
                }
                
                user.StudentDetail.TermsAndConditionsAccepted = true;
                // TODO: PDR ataması yapılacak. 
                user.StudentDetail.Status = StudentStatus.Active;
                    
                await dbContext.SaveChangesAsync();
                    
                return TypedResults.Ok();

            }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
            .WithTags("Öğrenci Tanışma Formu İşlemleri");
        
        Result RequestValidation(StudentOnboardRequest request)
        {
            var errorDescriptors = new List<ErrorDescriptor>();
            
            errorDescriptors.AddRange(ValidateStudentGeneralInfo(request.StudentGeneralInfo));
            
            errorDescriptors.AddRange(ValidateParentInfo(request.ParentInfo));

            errorDescriptors.AddRange(ValidateAcademicSummary(request.AcademicSummary));

            if (request.IsTryPracticeTYTExamBefore) // Temel Yeterlilik Testi
            {
                errorDescriptors.AddRange(ValidateLastPracticeTytExamPoints(request.LastPracticeTytExamPoints));
            }
            
            if (request.StudentGeneralInfo?.ExamType != ExamType.TYT && request.IsTryPracticeAYTExamBefore) // Alan Yeterlilik Testi
            {
                switch (request.StudentGeneralInfo.ExamType)
                {
                    case ExamType.TM:
                        errorDescriptors.AddRange(
                            ValidateLastPracticeTmExamPoints(request.LastPracticeTmExamPoints));
                        break;
                    case ExamType.MF:
                        errorDescriptors.AddRange(
                            ValidateLastPracticeMfExamPoints(request.LastPracticeMfExamPoints));
                        break;
                    case ExamType.Sozel:
                        errorDescriptors.AddRange(
                            ValidateLastPracticeSozelExamPoints(request.LastPracticeSozelExamPoints));
                        break;
                    case ExamType.Dil:
                        errorDescriptors.AddRange(
                            ValidateLastPracticeDilExamPoints(request.LastPracticeDilExamPoints));
                        break;
                }
            }
            
            //F Part
            errorDescriptors.AddRange(ValidateStudentGoals(request.StudentGoals));

            //G Part
            errorDescriptors.AddRange(ValidateSupplementaryMaterials(request.StudentGeneralInfo.ExamType, request.SupplementaryMaterials));

            return errorDescriptors.Any() ? Result.Failure(errorDescriptors) : Result.Success();
        }
    }

    private static List<ErrorDescriptor> ValidateAcademicSummary(AcademicSummary? request)
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
        }

        return errorDescriptors;
    }

    private static List<ErrorDescriptor> ValidateStudentGoals(StudentGoals request)
    {
        var errorDescriptors = new List<ErrorDescriptor>();
        
        if (request.TytGoalNet is < 0 or > 120)
        {
            errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidTYTGoalNet", "TYT neti",
                request.TytGoalNet.Value, 120, 0));
        }

        if (request.AytGoalNet is < 0 or > 80)
        {
            errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidAYTGoalNet", "AYT neti",
                request.AytGoalNet.Value, 80, 0));
        }

        if (request.GoalRanking is < 1 or > 5000000)
        {
            errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidGoalRanking", "Hedef sıralaması",
                request.GoalRanking.Value, 5000000, 1));
        }

        //request.DesiredProfessionSchoolField

        return errorDescriptors;
    }

    private static List<ErrorDescriptor> ValidateSupplementaryMaterials(ExamType examType, SupplementaryMaterials? request)
    {
        var errorDescriptors = new List<ErrorDescriptor>();
        
        // if (string.IsNullOrEmpty(examType) || !examTypes.Contains(examType.ToLower()))
        // {
        //     errorDescriptors.Add(CommonErrorDescriptor.InvalidExamType(examType));
        // }
        
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
                        switch (examType)
                        {
                            case ExamType.TM when request.PrivateTutoringAytLessons?.Tm is null:
                                errorDescriptors.Add(CommonErrorDescriptor.EmptyTM());
                                break;
                            case ExamType.MF when request.PrivateTutoringAytLessons?.Mf is null:
                                errorDescriptors.Add(CommonErrorDescriptor.EmptyMF());
                                break;
                            case ExamType.Sozel
                                when request.PrivateTutoringAytLessons?.Sozel is null:
                                errorDescriptors.Add(CommonErrorDescriptor.EmptySozel());
                                break;
                            case ExamType.Dil when request.PrivateTutoringAytLessons?.Dil is null:
                                errorDescriptors.Add(CommonErrorDescriptor.EmptyDil());
                                break;
                        }
                    }
                }
            }    
        }
        
        return errorDescriptors;
    }

    private static List<ErrorDescriptor> ValidateLastPracticeDilExamPoints(DilAveragePoints? request)
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

    private static List<ErrorDescriptor> ValidateLastPracticeSozelExamPoints(SozelAveragePoints? request)
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

            //Coğrafya-2: (Max 13, Min 0)
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

    private static List<ErrorDescriptor> ValidateLastPracticeMfExamPoints(MFAveragePoints? request)
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

            //Fizik: (Max 13, Min 0)
            if (request.Physics <= 13)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidPhysicsNet",
                    "Fizik neti",
                    request.Physics, 13, 0));
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

    private static List<ErrorDescriptor> ValidateLastPracticeTmExamPoints(TMAveragePoints? request)
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

    private static List<ErrorDescriptor> ValidateLastPracticeTytExamPoints(TYTAveragePoints? request)
    {
        var errorDescriptors = new List<ErrorDescriptor>();
        
        if (request is null)
        {
            errorDescriptors.Add(CommonErrorDescriptor.EmptyLastPracticeTYTExamPoints());
        }
        else
        {
            //Anlam Bilgisi: (Max 30, Min 0)
            if (request.Semantics > 30)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidSemanticNet", "Anlam bilgisi neti",
                    request.Semantics, 30, 0));
            }
            
            //Dil Bilgisi: (Max 10, Min 0)
            if (request.Grammar > 10)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidGrammarNet", "Dil bilgisi neti",
                    request.Grammar, 10, 0));
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
            
            //Kimya: (Max 7, Min 0)
            if (request.Chemistry > 7)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidRange("InvalidChemistryNet", "Kimya neti",
                    request.Chemistry, 7, 0));
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

    private static List<ErrorDescriptor> ValidateParentInfo(ParentInfo? request)
    {
        var errorDescriptors = new List<ErrorDescriptor>();

        if (request is null)
        {
            errorDescriptors.Add(CommonErrorDescriptor.EmptyParentInfo());
        }
        else
        {
            if (string.IsNullOrEmpty(request.Name) || request.Name.Length < 3)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidParentName(request.Name));
            }

            if (string.IsNullOrEmpty(request.Surname) || request.Surname.Length < 2)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidParentSurname(request.Surname));
            }

            if (string.IsNullOrEmpty(request.Email) || !EmailAddressAttribute.IsValid(request.Email))
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidParentEmail(request.Email));
            }

            if (string.IsNullOrEmpty(request.Mobile))
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidParentMobile(request.Mobile));
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
                    errorDescriptors.Add(CommonErrorDescriptor.InvalidParentMobile(request.Mobile));
                }
            }    
        }

        return errorDescriptors;
    }

    private static List<ErrorDescriptor> ValidateStudentGeneralInfo(StudentGeneralInfo? request)
    {
        var errorDescriptors = new List<ErrorDescriptor>();

        if (request is null)
        {
            errorDescriptors.Add(CommonErrorDescriptor.EmptyStudentGeneralInfo());
        }
        else
        {
            if (string.IsNullOrEmpty(request.Name) || request.Name.Length < 3)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidStudentName(request.Name));
            }

            if (string.IsNullOrEmpty(request.Surname) || request.Surname.Length < 2)
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidStudentSurname(request.Surname));
            }

            if (string.IsNullOrEmpty(request.Email) || !EmailAddressAttribute.IsValid(request.Email))
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidStudentEmail(request.Email));
            }

            if (string.IsNullOrEmpty(request.Mobile))
            {
                errorDescriptors.Add(CommonErrorDescriptor.InvalidStudentMobile(request.Mobile));
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
                    errorDescriptors.Add(CommonErrorDescriptor.InvalidStudentMobile(request.Mobile));
                }
            }    
            
            // if (string.IsNullOrEmpty(request.ExamType) || !examTypes.Contains(request.ExamType.ToLower()))
            // {
            //     errorDescriptors.Add(CommonErrorDescriptor.InvalidExamType(request.ExamType));
            // }
            //
            // if (string.IsNullOrEmpty(request.Grade) || !gradeTypes.Contains(request.Grade.ToLower()))
            // {
            //     errorDescriptors.Add(CommonErrorDescriptor.InvalidGrade(request.Grade));
            // }
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

    public class OnboardStudent
    {
        public ObjectId Id { get; set; }
        
        public string UserId { get; set; }
        
        public StudentOnboardRequest Data { get; set; }
    }

    /*
     * //Alan -> MF,TM,Sözel,Dil,Tyt
        public string ExamType { get; set; }
        
        //Sınıf -> 9-10-11-12 ve Mezun
        public string Grade  { get; set; }
     */
    
    public enum ExamType
    {
        TYT,
        TM,
        MF,
        Sozel,
        Dil,
    }

    public enum Grade
    {
        Dokuz,
        On,
        Onbir,
        Oniki,
        Mezun
    }

    public class StudentGeneralInfo
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ExamType ExamType { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Grade Grade { get; set; }
    }

    public record ParentInfo(string Name, string Surname, string Mobile, string Email);

    
    //Okuduğunuz lise
    //Lise orta öğretim başarı puanınız 0-100 arasında olmalı
    public record AcademicSummary(string HighSchool, float HighSchoolGPA);

    //TYT hedef netiniz: (Max 120, Min 0) (required değil)

    //AYT hedef netiniz: (Max 80, Min 0) (required değil)

    //Hedef sıralamanız: (1-SONSUZ rangeta integer alır) (required değil)

    //Hedef meslek/okul/bölümünüz: (Free text string alır) (required değil)
    public record StudentGoals(byte? TytGoalNet, byte? AytGoalNet, uint? GoalRanking, string DesiredProfessionSchoolField);

    public class StudentOnboardRequest
    {
        public StudentGeneralInfo? StudentGeneralInfo { get; set; }
        
        public ParentInfo? ParentInfo { get; set; }
        
        public AcademicSummary? AcademicSummary { get; set; }
        
        //Daha önce TYT denemesine girdiniz mi? True ise LastPracticeTytExamPoints dolu olmalı. False ise 
        public bool IsTryPracticeTYTExamBefore { get; set; }
        
        public TYTAveragePoints? LastPracticeTytExamPoints { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise
        //LastPracticeTytExamPoints dolu olmalı. False ise 
        public bool IsTryPracticeAYTExamBefore { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise && ExamType -> MF
        public MFAveragePoints? LastPracticeMfExamPoints { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise && ExamType -> TM
        public TMAveragePoints? LastPracticeTmExamPoints { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise && ExamType -> Sozel
        public SozelAveragePoints? LastPracticeSozelExamPoints { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise && ExamType -> Dil
        public DilAveragePoints? LastPracticeDilExamPoints { get; set; }
        
        public StudentGoals? StudentGoals { get; set; }
        
        public SupplementaryMaterials? SupplementaryMaterials { get; set; }
    
        //Koçluktan beklentin nedir? (Free text uzun paragraf)
        public string ExpectationsFromCoaching { get; set; }
    }

    public class TYTAveragePoints
    {
        //Anlam Bilgisi: (Max 30, Min 0)
        public byte Semantics { get; set; }
        //Dil Bilgisi: (Max 10, Min 0)
        public byte Grammar { get; set; }
        //Matematik: (Max 30, Min 0)
        public byte Mathematics { get; set; }
        //Geometri: (Max 10, Min 0)
        public byte Geometry { get; set; }
        //Tarih: (Max 5, Min 0)
        public byte History { get; set; }
        //Coğrafya: (Max 5, Min 0)
        public byte Geography { get; set; }
        //Felsefe: (Max 5, Min 0)
        public byte Philosophy { get; set; }
        //Din: (Max 5, Min 0)
        public byte Religion { get; set; }
        //Fizik: (Max 7, Min 0)
        public byte Physics { get; set; }
        //Kimya: (Max 7, Min 0)
        public byte Chemistry { get; set; }
        //Biology: (Max 6, Min 0)
        public byte Biology { get; set; }
    }
    
    public class MFAveragePoints
    {
        //Matematik: (Max 30, Min 0)
        public byte Mathematics { get; set; }
        //Geometri: (Max 10, Min 0)
        public byte Geometry { get; set; }
        //Fizik: (Max 14, Min 0)
        public byte Physics { get; set; }
        //Kimya: (Max 13, Min 0)
        public byte Chemistry { get; set; }
        //Biology: (Max 13, Min 0)
        public byte Biology { get; set; }
    }
    
    public class TMAveragePoints
    {
        //Matematik: (Max 30, Min 0)
        public byte Mathematics { get; set; }
        //Geometri: (Max 10, Min 0)
        public byte Geometry { get; set; }
        //Edebiyat: (Max 24, Min 0)
        public byte Literature { get; set; }
        //Tarih: (Max 10, Min 0)
        public byte History { get; set; }
        //Coğrafya: (Max 6, Min 0)
        public byte Geography { get; set; }
    }
    
    //Sözel
    public class SozelAveragePoints
    {
        //Tarih-1: (Max 10, Min 0)
        public byte History1 { get; set; }
        //Coğrafya: (Max 24, Min 0)
        public byte Geography1 { get; set; }
        //Edebiyat-1: (Max 6, Min 0)
        public byte Literature1 { get; set; }
        //Tarih-2: (Max 11, Min 0)
        public byte History2 { get; set; }
        //Coğrafya-2: (Max 11, Min 0)
        public byte Geography2 { get; set; }
        //Felsefe: (Max 12, Min 0)
        public byte Philosophy { get; set; }
        //Din: (Max 6, Min 0)
        public byte Religion { get; set; }
    }
    
    public class DilAveragePoints
    {
        //YDT: (Max 80, Min 0) Yabacnı Dil Testi
        public byte YDT { get; set; }
    }
    
    public class SupplementaryMaterials
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
        public PrivateTutoringTYT? PrivateTutoringTytLessons { get; set; }
        
        public bool PrivateTutoringAyt { get; set; }
        //AYT(Başlık):
        //Alanına göre a partında seçtiği dersler gelir. Hangi alandan hangi derslerin geleceğini e partındaki netler kısmından ulaşabilirsiniz.
        public PrivateTutoringAYT? PrivateTutoringAytLessons { get; set; }
        
        public bool Course { get; set; }
        public bool Youtube { get; set; }
    }

    public class PrivateTutoringTYT
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
    
    public class PrivateTutoringAYT
    {
        public MF? Mf { get; set; }
        public TM? Tm { get; set; }
        public Sozel? Sozel { get; set; }
        public Dil? Dil { get; set; }
    }
    
    public class MF
    {
        public bool Mathematics { get; set; }
        public bool Geometry { get; set; }
        public bool Physics { get; set; }
        public bool Chemistry { get; set; }
        public bool Biology { get; set; }
    }
    
    public class TM
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
    public class Sozel
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
    
    public class Dil
    {
        //YDT: (Max 80, Min 0)
        public bool YTD { get; set; }
    }
}