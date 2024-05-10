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
using Swashbuckle.AspNetCore.Filters;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public static class Onboard
{
    private static readonly EmailAddressAttribute EmailAddressAttribute = new();
    
    public static void MapOnboard(this IEndpointRouteBuilder endpoints)
    {
        // endpoints.MapPost("/onboard/student-general-info", Results<Ok, ProblemHttpResult>
        //         (StudentGeneralInfo request, [FromServices] IServiceProvider sp) =>
        //     {
        //         var requestValidation = ValidateStudentGeneralInfo(request);
        //
        //         if (requestValidation.Any())
        //         {
        //             return requestValidation.CreateProblem("Öğrenci bilgisi doğrulanamadı");
        //         }
        //
        //         return TypedResults.Ok();
        //
        //     }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
        //     .WithTags("Öğrenci Tanışma Formu İşlemleri");
        //
        // endpoints.MapPost("/onboard/parent-info", Results<Ok, ProblemHttpResult>
        //         (ParentInfo request, [FromServices] IServiceProvider sp) =>
        //     {
        //         var requestValidation = ValidateParentInfo(request);
        //
        //         if (requestValidation.Any())
        //         {
        //             return requestValidation.CreateProblem("Veli bilgisi doğrulanamadı");
        //         }
        //
        //         return TypedResults.Ok();
        //
        //     }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
        //     .WithTags("Öğrenci Tanışma Formu İşlemleri");
        //
        // endpoints.MapPost("/onboard/last-practice-tyt-exam-points", Results<Ok, ProblemHttpResult>
        //         (TYTAveragePoints request, [FromServices] IServiceProvider sp) =>
        //     {
        //         var requestValidation = ValidateLastPracticeTytExamPoints(request);
        //
        //         if (requestValidation.Any())
        //         {
        //             return requestValidation.CreateProblem("Son temel yeterlilik sınav türü bilgisi doğrulanamadı");
        //         }
        //
        //         return TypedResults.Ok();
        //
        //     }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
        //     .WithTags("Öğrenci Tanışma Formu İşlemleri");
        //
        // endpoints.MapPost("/onboard/last-practice-tm-exam-points", Results<Ok, ProblemHttpResult>
        //         (TMAveragePoints request, [FromServices] IServiceProvider sp) =>
        //     {
        //         var requestValidation = ValidateLastPracticeTmExamPoints(request);
        //
        //         if (requestValidation.Any())
        //         {
        //             return requestValidation.CreateProblem("Son eşit ağırlık puan türü bilgisi doğrulanamadı");
        //         }
        //
        //         return TypedResults.Ok();
        //
        //     }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
        //     .WithTags("Öğrenci Tanışma Formu İşlemleri");
        //
        // endpoints.MapPost("/onboard/last-practice-mf-exam-points", Results<Ok, ProblemHttpResult>
        //         (MFAveragePoints request, [FromServices] IServiceProvider sp) =>
        //     {
        //         var requestValidation = ValidateLastPracticeMfExamPoints(request);
        //
        //         if (requestValidation.Any())
        //         {
        //             return requestValidation.CreateProblem("Son sayısal puan türü bilgisi doğrulanamadı");
        //         }
        //
        //         return TypedResults.Ok();
        //
        //     }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
        //     .WithTags("Öğrenci Tanışma Formu İşlemleri");
        //
        // endpoints.MapPost("/onboard/last-practice-sozel-exam-points", Results<Ok, ProblemHttpResult>
        //         (SozelAveragePoints request, [FromServices] IServiceProvider sp) =>
        //     {
        //         var requestValidation = ValidateLastPracticeSozelExamPoints(request);
        //
        //         if (requestValidation.Any())
        //         {
        //             return requestValidation.CreateProblem("Son sözel puan türü bilgisi doğrulanamadı");
        //         }
        //
        //         return TypedResults.Ok();
        //
        //     }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
        //     .WithTags("Öğrenci Tanışma Formu İşlemleri");
        //
        // endpoints.MapPost("/onboard/last-practice-dil-exam-points", Results<Ok, ProblemHttpResult>
        //         (DilAveragePoints request, [FromServices] IServiceProvider sp) =>
        //     {
        //         var requestValidation = ValidateLastPracticeDilExamPoints(request);
        //
        //         if (requestValidation.Any())
        //         {
        //             return requestValidation.CreateProblem("Son yabancı dil puan türü bilgisi doğrulanamadı");
        //         }
        //
        //         return TypedResults.Ok();
        //
        //     }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
        //     .WithTags("Öğrenci Tanışma Formu İşlemleri");
        //
        // endpoints.MapPost("/onboard/student-goals", Results<Ok, ProblemHttpResult>
        //         (StudentGoals request, [FromServices] IServiceProvider sp) =>
        //     {
        //         var requestValidation = ValidateStudentGoals(request);
        //
        //         if (requestValidation.Any())
        //         {
        //             return requestValidation.CreateProblem("Öğrenci hedefleri doğrulanamadı.");
        //         }
        //
        //         return TypedResults.Ok();
        //
        //     }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
        //     .WithTags("Öğrenci Tanışma Formu İşlemleri");
        //
        // endpoints.MapPost("/onboard/academic-summary", Results<Ok, ProblemHttpResult>
        //         (AcademicSummary request, [FromServices] IServiceProvider sp) =>
        //     {
        //         var requestValidation = ValidateAcademicSummary(request);
        //
        //         if (requestValidation.Any())
        //         {
        //             return requestValidation.CreateProblem("Öğrenci akademik özet bilgisi doğrulanamadı.");
        //         }
        //
        //         return TypedResults.Ok();
        //
        //     }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
        //     .WithTags("Öğrenci Tanışma Formu İşlemleri");
        //
        // endpoints.MapPost("/onboard/suplementary-materials/{examType}", Results<Ok, ProblemHttpResult>
        //     ([FromQuery] ExamType examType, [FromBody] SupplementaryMaterials request,
        //         [FromServices] IServiceProvider sp) =>
        //     {
        //         var requestValidation = ValidateSupplementaryMaterials(examType, request);
        //
        //         if (requestValidation.Any())
        //         {
        //             return requestValidation.CreateProblem("Öğrenci akademik özet bilgisi doğrulanamadı.");
        //         }
        //
        //         return TypedResults.Ok();
        //
        //     }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
        //     .WithTags("Öğrenci Tanışma Formu İşlemleri");

        endpoints.MapPost("/onboard", async Task<Results<Ok, ProblemHttpResult>>
                (StudentOnboardRequest request, ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                var requestValidation = RequestValidation(request);

                if (requestValidation.IsFailure)
                {
                    var errors = requestValidation.Errors;

                    return TypedResults.Problem("Tanışma formu eksik yada hatalı!",
                        statusCode: StatusCodes.Status400BadRequest,
                        extensions: new Dictionary<string, object?>()
                        {
                            { "errors", errors },
                        });
                }

                var studentId = claimsPrincipal.GetId();

                var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                // var mongoDatabase = sp.GetRequiredService<IMongoDatabase>();
                //
                // var onboardStudentCollection = mongoDatabase.GetCollection<OnboardStudent>("onboardStudents");

                // var strUserId = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
                //
                // var filter = Builders<OnboardStudent>.Filter.Eq(s => s.UserId, strUserId);

                // var onboardStudent = await onboardStudentCollection.Find(filter).FirstOrDefaultAsync();

                var student =
                    await dbContext.Users
                        .Include(x => x.StudentDetail)
                        .Include(x => x.SubscriptionHistories)
                        .FirstOrDefaultAsync(x => x.Id == studentId && x.UserType == UserType.Student);

                student.StudentDetail.Name = request.StudentGeneralInfo.Name;
                student.StudentDetail.Surname = request.StudentGeneralInfo.Surname;
                student.StudentDetail.Email = request.StudentGeneralInfo.Email;
                student.StudentDetail.Mobile = request.StudentGeneralInfo.Mobile;
                student.StudentDetail.Grade = request.StudentGeneralInfo.Grade;
                student.StudentDetail.ExamType = request.StudentGeneralInfo.ExamType;
                
                student.StudentDetail.ParentName = request.ParentInfo.Name;
                student.StudentDetail.ParentSurname = request.ParentInfo.Surname;
                student.StudentDetail.ParentEmail = request.ParentInfo.Email;
                student.StudentDetail.ParentMobile = request.ParentInfo.Mobile;
                
                student.StudentDetail.HighSchool = request.AcademicSummary.HighSchool;
                student.StudentDetail.HighSchoolGPA = request.AcademicSummary.HighSchoolGPA;

                student.StudentDetail.GoalRanking = request.StudentGoals.GoalRanking;
                student.StudentDetail.TytGoalNet = request.StudentGoals.TytGoalNet;
                student.StudentDetail.AytGoalNet = request.StudentGoals.AytGoalNet;
                student.StudentDetail.DesiredProfessionSchoolField = request.StudentGoals.DesiredProfessionSchoolField;

                student.StudentDetail.ExpectationsFromCoaching = request.ExpectationsFromCoaching;

                student.StudentDetail.Course = request.SupplementaryMaterials.Course;
                student.StudentDetail.School = request.SupplementaryMaterials.School;
                student.StudentDetail.Youtube = request.SupplementaryMaterials.Youtube;

                if (request.SupplementaryMaterials.PrivateTutoring)
                {
                    student.StudentDetail.PrivateTutoringTyt = request.SupplementaryMaterials.PrivateTutoringTyt;
                    student.StudentDetail.PrivateTutoringAyt = request.SupplementaryMaterials.PrivateTutoringAyt;
                    
                    if (request.SupplementaryMaterials.PrivateTutoringTyt)
                    {
                        student.PrivateTutoringTYT = new PrivateTutoringTYT()
                        {
                            Biology = request.SupplementaryMaterials.PrivateTutoringTytLessons.Biology,
                            Chemistry = request.SupplementaryMaterials.PrivateTutoringTytLessons.Chemistry,
                            Geography = request.SupplementaryMaterials.PrivateTutoringTytLessons.Geography,
                            Geometry = request.SupplementaryMaterials.PrivateTutoringTytLessons.Geometry,
                            History = request.SupplementaryMaterials.PrivateTutoringTytLessons.History,
                            Mathematics = request.SupplementaryMaterials.PrivateTutoringTytLessons.Mathematics,
                            Philosophy = request.SupplementaryMaterials.PrivateTutoringTytLessons.Philosophy,
                            Physics = request.SupplementaryMaterials.PrivateTutoringTytLessons.Physics,
                            Religion = request.SupplementaryMaterials.PrivateTutoringTytLessons.Religion,
                            Turkish = request.SupplementaryMaterials.PrivateTutoringTytLessons.Turkish,
                        };
                    }

                    if (request.SupplementaryMaterials.PrivateTutoringAyt)
                    {
                        if (student.StudentDetail.ExamType == ExamType.TM)
                        {
                            student.PrivateTutoringTM = new PrivateTutoringTM()
                            {
                                Geography = request.SupplementaryMaterials.PrivateTutoringAytLessons.Tm.Geography,
                                Geometry = request.SupplementaryMaterials.PrivateTutoringAytLessons.Tm.Geometry,
                                History = request.SupplementaryMaterials.PrivateTutoringAytLessons.Tm.History,
                                Literature = request.SupplementaryMaterials.PrivateTutoringAytLessons.Tm.Literature,
                                Mathematics = request.SupplementaryMaterials.PrivateTutoringAytLessons.Tm.Mathematics,
                            };
                        }
                        else if (student.StudentDetail.ExamType == ExamType.MF)
                        {
                            student.PrivateTutoringMF = new PrivateTutoringMF()
                            {
                                Biology = request.SupplementaryMaterials.PrivateTutoringAytLessons.Mf.Biology,
                                Chemistry = request.SupplementaryMaterials.PrivateTutoringAytLessons.Mf.Chemistry,
                                Geometry = request.SupplementaryMaterials.PrivateTutoringAytLessons.Mf.Geometry,
                                Mathematics = request.SupplementaryMaterials.PrivateTutoringAytLessons.Mf.Mathematics,
                                Physics = request.SupplementaryMaterials.PrivateTutoringAytLessons.Mf.Physics,
                            };
                        }
                        else if (student.StudentDetail.ExamType == ExamType.Sozel)
                        {
                            student.PrivateTutoringSozel = new PrivateTutoringSozel()
                            {
                                Geography1 = request.SupplementaryMaterials.PrivateTutoringAytLessons.Sozel.Geography1,
                                Geography2 = request.SupplementaryMaterials.PrivateTutoringAytLessons.Sozel.Geography2,
                                History1 = request.SupplementaryMaterials.PrivateTutoringAytLessons.Sozel.History1,
                                History2 = request.SupplementaryMaterials.PrivateTutoringAytLessons.Sozel.History2,
                                Literature1 = request.SupplementaryMaterials.PrivateTutoringAytLessons.Sozel.Literature1,
                                Philosophy = request.SupplementaryMaterials.PrivateTutoringAytLessons.Sozel.Philosophy,
                                Religion = request.SupplementaryMaterials.PrivateTutoringAytLessons.Sozel.Religion,
                            };
                        }
                        else if (student.StudentDetail.ExamType == ExamType.Dil)
                        {
                            student.PrivateTutoringDil = new PrivateTutoringDil()
                            {
                                YTD = request.SupplementaryMaterials.PrivateTutoringAytLessons.Dil.YTD
                            };
                        }
                    }
                }
                
                if (request.IsTryPracticeTYTExamBefore)
                {
                    student.TytNets = new TYTNets()
                    {
                        Biology = request.LastPracticeTytExamPoints.Biology,
                        Chemistry = request.LastPracticeTytExamPoints.Chemistry,
                        Geography = request.LastPracticeTytExamPoints.Geography,
                        Geometry = request.LastPracticeTytExamPoints.Geometry,
                        Grammar = request.LastPracticeTytExamPoints.Grammar,
                        History = request.LastPracticeTytExamPoints.History,
                        Mathematics = request.LastPracticeTytExamPoints.Mathematics,
                        Philosophy = request.LastPracticeTytExamPoints.Philosophy,
                        Physics = request.LastPracticeTytExamPoints.Physics,
                        Religion = request.LastPracticeTytExamPoints.Religion,
                        Semantics = request.LastPracticeTytExamPoints.Semantics,
                    };
                }

                if (request.IsTryPracticeAYTExamBefore)
                {
                    if (request.StudentGeneralInfo.ExamType == ExamType.TM)
                    {
                        student.TmNets = new TMNets()
                        {
                            Geography = request.LastPracticeTmExamPoints.Geography,
                            Geometry = request.LastPracticeTmExamPoints.Geometry,
                            History = request.LastPracticeTmExamPoints.History,
                            Literature = request.LastPracticeTmExamPoints.Literature,
                            Mathematics = request.LastPracticeTmExamPoints.Mathematics,
                        };
                    }
                    else if (request.StudentGeneralInfo.ExamType == ExamType.MF)
                    {
                        student.MfNets = new MFNets()
                        {
                            Biology = request.LastPracticeMfExamPoints.Biology,
                            Chemistry = request.LastPracticeMfExamPoints.Chemistry,
                            Geometry = request.LastPracticeMfExamPoints.Geometry,
                            Mathematics = request.LastPracticeMfExamPoints.Mathematics,
                            Physics = request.LastPracticeMfExamPoints.Physics,
                        };
                    }
                    else if (request.StudentGeneralInfo.ExamType == ExamType.Sozel)
                    {
                        student.SozelNets = new SozelNets()
                        {
                            Geography1 = request.LastPracticeSozelExamPoints.Geography1,
                            Geography2 = request.LastPracticeSozelExamPoints.Geography2,
                            History1 = request.LastPracticeSozelExamPoints.History1,
                            History2 = request.LastPracticeSozelExamPoints.History2,
                            Literature1 = request.LastPracticeSozelExamPoints.Literature1,
                            Philosophy = request.LastPracticeSozelExamPoints.Philosophy,
                            Religion = request.LastPracticeSozelExamPoints.Religion,
                        };
                    }
                    else if (request.StudentGeneralInfo.ExamType == ExamType.Dil)
                    {
                        student.DilNets = new DilNets()
                        {
                            YDT = request.LastPracticeDilExamPoints.YDT,
                        };
                    }
                }
                
                var subscription = student.SubscriptionHistories.Where(x => x.SubscriptionEndDate != null)
                    .OrderBy(x => x.SubscriptionEndDate.Value)
                    .FirstOrDefault(x => x.SubscriptionEndDate.Value > DateTime.UtcNow);

                if (subscription.Type == SubscriptionType.Pdr)
                {
                    student!.StudentDetail.Status = StudentStatus.Active;
                }
                else if (subscription.Type == SubscriptionType.Coach)
                {
                    student!.StudentDetail.Status = StudentStatus.CoachSelect;
                }

                await dbContext.SaveChangesAsync();

                // if (onboardStudent is null)
                // {
                //     await onboardStudentCollection.InsertOneAsync(new OnboardStudent()
                //     {
                //         UserId = strUserId,
                //         Data = request,
                //     });
                //
                //     Guid.TryParse(strUserId, out var userId);
                //
                //     var user = await dbContext.Users
                //         .Include(x => x.StudentDetail)
                //         .Include(x => x.SubscriptionHistories)
                //         .FirstOrDefaultAsync(x => x.Id == userId);
                //
                //     if (user is not null)
                //     {
                //         var subscription = user.SubscriptionHistories.Where(x => x.SubscriptionEndDate != null)
                //             .OrderBy(x => x.SubscriptionEndDate.Value)
                //             .FirstOrDefault(x => x.SubscriptionEndDate.Value > DateTime.UtcNow);
                //         
                //         if (subscription.Type == SubscriptionType.Pdr)
                //         {
                //             user!.StudentDetail.Status = StudentStatus.Active;    
                //         }else if (subscription.Type == SubscriptionType.Coach)
                //         {
                //             user!.StudentDetail.Status = StudentStatus.CoachSelect;
                //         }
                //         
                //         await dbContext.SaveChangesAsync();
                //     }
                // }
                // else
                // {
                //     filter = Builders<OnboardStudent>.Filter.Eq(s => s.Id, onboardStudent.Id);
                //
                //     await onboardStudentCollection.ReplaceOneAsync(filter, new OnboardStudent()
                //     {
                //         Id = onboardStudent.Id,
                //         UserId = strUserId,
                //         Data = request,
                //     });
                // }

                return TypedResults.Ok();
            }).RequireAuthorization("Student").Produces<ProblemHttpResult>(StatusCodes.Status400BadRequest)
            .WithOpenApi()
            .WithTags("Öğrenci Tanışma Formu İşlemleri");
        
        // endpoints.MapPost("/onboard/terms-and-conditions-accepted", async Task<Results<Ok, ProblemHttpResult>>
        //         (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
        //     {
        //         var dbContext = sp.GetRequiredService<ApplicationDbContext>();
        //         
        //         var strUserId = claimsPrincipal?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        //
        //         Guid.TryParse(strUserId, out var userId);
        //
        //         var user = dbContext.Users
        //             .Include(x => x.StudentDetail)
        //             // .Include(x => x.SubscriptionHistories)
        //             .FirstOrDefault(x => x.Id == userId && x.UserType == UserType.Student);
        //
        //         // var subscription = user!.SubscriptionHistories.OrderBy(x => x.SubscriptionEndDate)
        //         //     .FirstOrDefault(
        //         //         x => x.SubscriptionEndDate.HasValue && x.SubscriptionEndDate.Value > DateTime.UtcNow);
        //         
        //         // if (user.StudentDetail.Status == StudentStatus.OnboardCompleted && subscription is not null && subscription.Type == SubscriptionType.OnlyPdr)
        //
        //         if (user is not null && user.StudentDetail.Status != StudentStatus.OnboardCompleted)
        //         {
        //             return TypedResults.Problem("Öğrenci tanışma formunu tamamlayın",
        //                 statusCode: StatusCodes.Status400BadRequest);
        //         }
        //         
        //         user.StudentDetail.TermsAndConditionsAccepted = true;
        //         // TODO: PDR ataması yapılacak. 
        //         user.StudentDetail.Status = StudentStatus.Active;
        //             
        //         await dbContext.SaveChangesAsync();
        //             
        //         return TypedResults.Ok();
        //
        //     }).RequireAuthorization().Produces<ProblemHttpResult>(400).WithOpenApi()
        //     .WithTags("Öğrenci Tanışma Formu İşlemleri");
        
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

    public class ParentInfo
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
    }

    //Okuduğunuz lise
    //Lise orta öğretim başarı puanınız 0-100 arasında olmalı
    public class AcademicSummary
    {
        public string HighSchool { get; set; }
        public float HighSchoolGPA { get; set; }
    }

    //TYT hedef netiniz: (Max 120, Min 0) (required değil)

    //AYT hedef netiniz: (Max 80, Min 0) (required değil)

    //Hedef sıralamanız: (1-SONSUZ rangeta integer alır) (required değil)

    //Hedef meslek/okul/bölümünüz: (Free text string alır) (required değil)
    public class StudentGoals
    {
        public byte? TytGoalNet { get; set; }
        public byte? AytGoalNet { get; set; }
        public uint? GoalRanking { get; set; }
        public string? DesiredProfessionSchoolField { get; set; }   
    }

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
        
        public ResourcesTYT? ResourcesTYT { get; set; }
        
        public ResourcesAYT? ResourcesAYT { get; set; }
        
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

    public class ResourcesTYT
    {
        public string Turkish { get; set; }
        public string Mathematics { get; set; }
        public string Geometry { get; set; }
        public string History { get; set; }
        public string Geography { get; set; }
        public string Philosophy { get; set; }
        public string Religion { get; set; }
        public string Physics { get; set; }
        public string Chemistry { get; set; }
        public string Biology { get; set; }
    }
    
    public class ResourcesAYT
    {
        public string Mathematics { get; set; }
        public string Geometry { get; set; }
        public string Physics { get; set; }
        public string Chemistry { get; set; }
        public string Biology { get; set; }
        public string Literature { get; set; }
        public string History { get; set; }
        public string Geography { get; set; }
        public string Philosophy { get; set; }
        public string Religion { get; set; }
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
        public PrivateTutoringTYTObject? PrivateTutoringTytLessons { get; set; }
        
        public bool PrivateTutoringAyt { get; set; }
        //AYT(Başlık):
        //Alanına göre a partında seçtiği dersler gelir. Hangi alandan hangi derslerin geleceğini e partındaki netler kısmından ulaşabilirsiniz.
        public PrivateTutoringAYTObject? PrivateTutoringAytLessons { get; set; }
        
        public bool Course { get; set; }
        public bool Youtube { get; set; }
    }

    public class PrivateTutoringTYTObject
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
    
    public class PrivateTutoringAYTObject
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
    
    public class StudentOnboardRequestExamples : IMultipleExamplesProvider<StudentOnboardRequest>
    {
        private StudentGeneralInfo servetStudentGeneralInfoTYT = new StudentGeneralInfo()
        {
            Name = "Servet",
            Surname = "ŞEKER",
            Email = "servetseker@gmail.com",
            Grade = Grade.Oniki,
            ExamType = ExamType.TYT,
            Mobile = "533-333-33-33",
        };
        
        private StudentGeneralInfo servetStudentGeneralInfoMF = new StudentGeneralInfo()
        {
            Name = "Servet",
            Surname = "ŞEKER",
            Email = "servetseker@gmail.com",
            Grade = Grade.Oniki,
            ExamType = ExamType.MF,
            Mobile = "533-333-33-33",
        };
        
        private StudentGeneralInfo servetStudentGeneralInfoTM = new StudentGeneralInfo()
        {
            Name = "Servet",
            Surname = "ŞEKER",
            Email = "servetseker@gmail.com",
            Grade = Grade.Oniki,
            ExamType = ExamType.TM,
            Mobile = "533-333-33-33",
        };
        
        private StudentGeneralInfo servetStudentGeneralInfoSozel = new StudentGeneralInfo()
        {
            Name = "Servet",
            Surname = "ŞEKER",
            Email = "servetseker@gmail.com",
            Grade = Grade.Oniki,
            ExamType = ExamType.Sozel,
            Mobile = "533-333-33-33",
        };
        
        private StudentGeneralInfo servetStudentGeneralInfoDil = new StudentGeneralInfo()
        {
            Name = "Servet",
            Surname = "ŞEKER",
            Email = "servetseker@gmail.com",
            Grade = Grade.Oniki,
            ExamType = ExamType.Dil,
            Mobile = "533-333-33-33",
        };

        private ParentInfo servetParentInfo = new ParentInfo()
        {
            Name = "Hacı",
            Surname = "ŞEKER",
            Email = "haciseker@gmail.com",
            Mobile = "533-333-33-33"
        };

        private AcademicSummary servetAcademicSummary = new AcademicSummary()
        {
            HighSchool = "Güngören Anadolu Teknik Lisesi",
            HighSchoolGPA = 91.4f
        };

        private StudentGoals emptyStudentGoals = new StudentGoals()
        {
            TytGoalNet = null,
            AytGoalNet = null,
            GoalRanking = null,
            DesiredProfessionSchoolField = String.Empty,
        };

        private SupplementaryMaterials onlySchoolSupplementaryMaterials = new SupplementaryMaterials()
        {
            Course = false,
            PrivateTutoring = false,
            Youtube = false,
            School = true,
        };
        
        private SupplementaryMaterials privateTutoringTytSupplementaryMaterials = new SupplementaryMaterials()
        {
            Course = false,
            PrivateTutoring = true,
            Youtube = false,
            School = true,
            PrivateTutoringTyt = true,
            PrivateTutoringTytLessons = new PrivateTutoringTYTObject()
            {
                Turkish = true,
                Mathematics = true,
                Geometry = false,
                History = false,
                Geography = true,
                Philosophy = false,
                Religion = false,
                Physics = true,
                Chemistry = false,
                Biology = true,
            }
        };

        private TYTAveragePoints validTytAveragePoints = new TYTAveragePoints()
        {
            Semantics = 22,
            Grammar = 7,
            Mathematics = 17,
            Geometry = 6,
            History = 3,
            Geography = 2,
            Philosophy = 2,
            Religion = 3,
            Physics = 1,
            Chemistry = 1,
            Biology = 4,
        };
        
        private TYTAveragePoints invalidTytAveragePoints = new TYTAveragePoints()
        {
            Semantics = 34,
            Grammar = 22,
            Mathematics = 31,
            Geometry = 18,
            History = 7,
            Geography = 7,
            Philosophy = 6,
            Religion = 8,
            Physics = 10,
            Chemistry = 12,
            Biology = 7,
        };
        
        private MFAveragePoints validMfAveragePoints = new MFAveragePoints()
        {
            Mathematics = 27,
            Geometry = 8,
            Physics = 12,
            Chemistry = 11,
            Biology = 10
        };

        private MFAveragePoints invalidMfAveragePoints = new MFAveragePoints()
        {
            Mathematics = 33,
            Geometry = 12,
            Physics = 17,
            Chemistry = 15,
            Biology = 20
        };
        
        private TMAveragePoints validTmAveragePoints = new TMAveragePoints()
        {
            Mathematics = 18,
            Geometry = 4,
            Literature = 18,
            History = 7,
            Geography = 4
        };

        private TMAveragePoints invalidTmAveragePoints = new TMAveragePoints()
        {
            Mathematics = 37,
            Geometry = 13,
            Literature = 28,
            History = 12,
            Geography = 9
        };
        
        private SozelAveragePoints validSozelAveragePoints = new SozelAveragePoints()
        {
            History1 = 8,
            Geography1 = 22,
            Literature1 = 4,
            History2 = 7,
            Geography2 = 7,
            Philosophy = 7,
            Religion = 5,
        };

        private SozelAveragePoints invalidSozelAveragePoints = new SozelAveragePoints()
        {
            History1 = 13,
            Geography1 = 26,
            Literature1 = 9,
            History2 = 13,
            Geography2 = 14,
            Philosophy = 15,
            Religion = 11,
        };

        private DilAveragePoints validDilAveragePoints = new DilAveragePoints()
        {
            YDT = 65
        };
        
        private DilAveragePoints invalidDilAveragePoints = new DilAveragePoints()
        {
            YDT = 105
        };
        
        public IEnumerable<SwaggerExample<StudentOnboardRequest>> GetExamples()
        {
            yield return SwaggerExample.Create("Servet,Exam:TYT,TYT:false,AYT:false,Goals:Empty,SupplementaryMaterials:onlySchool", new StudentOnboardRequest()
            {
                StudentGeneralInfo = servetStudentGeneralInfoTYT,
                ParentInfo = servetParentInfo,
                AcademicSummary = servetAcademicSummary,
                IsTryPracticeTYTExamBefore = false,
                IsTryPracticeAYTExamBefore = false,
                StudentGoals = emptyStudentGoals,
                SupplementaryMaterials = onlySchoolSupplementaryMaterials,
                ExpectationsFromCoaching = ":)"
            });
            
            yield return SwaggerExample.Create("Servet,Exam:TYT,TYT:false,AYT:false,Goals:Empty,SupplementaryMaterials:privateTutoringTyt", new StudentOnboardRequest()
            {
                StudentGeneralInfo = servetStudentGeneralInfoTYT,
                ParentInfo = servetParentInfo,
                AcademicSummary = servetAcademicSummary,
                IsTryPracticeTYTExamBefore = false,
                IsTryPracticeAYTExamBefore = false,
                StudentGoals = emptyStudentGoals,
                SupplementaryMaterials = privateTutoringTytSupplementaryMaterials,
                ExpectationsFromCoaching = ":)"
            });
            
            yield return SwaggerExample.Create("Servet,Exam:TYT,TYT:true,AYT:false,Goals:Empty,SupplementaryMaterials:privateTutoringTyt", new StudentOnboardRequest()
            {
                StudentGeneralInfo = servetStudentGeneralInfoTYT,
                ParentInfo = servetParentInfo,
                AcademicSummary = servetAcademicSummary,
                IsTryPracticeTYTExamBefore = true,
                LastPracticeTytExamPoints = validTytAveragePoints,
                IsTryPracticeAYTExamBefore = false,
                StudentGoals = emptyStudentGoals,
                SupplementaryMaterials = privateTutoringTytSupplementaryMaterials,
                ExpectationsFromCoaching = ":)"
            });
            
            yield return SwaggerExample.Create("Servet,Exam:TYT,TYT:true-invalidPoints,AYT:false,Goals:Empty,SupplementaryMaterials:privateTutoringTyt", new StudentOnboardRequest()
            {
                StudentGeneralInfo = servetStudentGeneralInfoTYT,
                ParentInfo = servetParentInfo,
                AcademicSummary = servetAcademicSummary,
                IsTryPracticeTYTExamBefore = true,
                LastPracticeTytExamPoints = invalidTytAveragePoints,
                IsTryPracticeAYTExamBefore = false,
                StudentGoals = emptyStudentGoals,
                SupplementaryMaterials = privateTutoringTytSupplementaryMaterials,
                ExpectationsFromCoaching = ":)"
            });
            
            yield return SwaggerExample.Create("Servet,Exam:MF,TYT:false,AYT:false,Goals:Empty,SupplementaryMaterials:privateTutoringTyt", new StudentOnboardRequest()
            {
                StudentGeneralInfo = servetStudentGeneralInfoMF,
                ParentInfo = servetParentInfo,
                AcademicSummary = servetAcademicSummary,
                IsTryPracticeTYTExamBefore = false,
                IsTryPracticeAYTExamBefore = false,
                StudentGoals = emptyStudentGoals,
                SupplementaryMaterials = privateTutoringTytSupplementaryMaterials,
                ExpectationsFromCoaching = ":)"
            });
            
            yield return SwaggerExample.Create("Servet,Exam:MF,TYT:true,AYT:false,Goals:Empty,SupplementaryMaterials:privateTutoringTyt", new StudentOnboardRequest()
            {
                StudentGeneralInfo = servetStudentGeneralInfoMF,
                ParentInfo = servetParentInfo,
                AcademicSummary = servetAcademicSummary,
                IsTryPracticeTYTExamBefore = true,
                LastPracticeTytExamPoints = validTytAveragePoints,
                IsTryPracticeAYTExamBefore = false,
                StudentGoals = emptyStudentGoals,
                SupplementaryMaterials = privateTutoringTytSupplementaryMaterials,
                ExpectationsFromCoaching = ":)"
            });
            
            yield return SwaggerExample.Create("Servet,Exam:MF,TYT:true,AYT:true,Goals:Empty,SupplementaryMaterials:privateTutoringTyt", new StudentOnboardRequest()
            {
                StudentGeneralInfo = servetStudentGeneralInfoMF,
                ParentInfo = servetParentInfo,
                AcademicSummary = servetAcademicSummary,
                IsTryPracticeTYTExamBefore = true,
                LastPracticeTytExamPoints = validTytAveragePoints,
                IsTryPracticeAYTExamBefore = true,
                LastPracticeMfExamPoints = validMfAveragePoints,
                StudentGoals = emptyStudentGoals,
                SupplementaryMaterials = privateTutoringTytSupplementaryMaterials,
                ExpectationsFromCoaching = ":)"
            });
            
            yield return SwaggerExample.Create("Servet,Exam:MF,TYT:true,AYT:true-invalid,Goals:Empty,SupplementaryMaterials:privateTutoringTyt", new StudentOnboardRequest()
            {
                StudentGeneralInfo = servetStudentGeneralInfoMF,
                ParentInfo = servetParentInfo,
                AcademicSummary = servetAcademicSummary,
                IsTryPracticeTYTExamBefore = true,
                LastPracticeTytExamPoints = validTytAveragePoints,
                IsTryPracticeAYTExamBefore = true,
                LastPracticeMfExamPoints = invalidMfAveragePoints,
                StudentGoals = emptyStudentGoals,
                SupplementaryMaterials = privateTutoringTytSupplementaryMaterials,
                ExpectationsFromCoaching = ":)"
            });
            
            yield return SwaggerExample.Create("Servet,Exam:TM,TYT:true,AYT:true,Goals:Empty,SupplementaryMaterials:privateTutoringTyt", new StudentOnboardRequest()
            {
                StudentGeneralInfo = servetStudentGeneralInfoTM,
                ParentInfo = servetParentInfo,
                AcademicSummary = servetAcademicSummary,
                IsTryPracticeTYTExamBefore = true,
                LastPracticeTytExamPoints = validTytAveragePoints,
                IsTryPracticeAYTExamBefore = true,
                LastPracticeTmExamPoints = validTmAveragePoints,
                StudentGoals = emptyStudentGoals,
                SupplementaryMaterials = privateTutoringTytSupplementaryMaterials,
                ExpectationsFromCoaching = ":)"
            });
    
            yield return SwaggerExample.Create("Servet,Exam:TM,TYT:true,AYT:true-invalid,Goals:Empty,SupplementaryMaterials:privateTutoringTyt", new StudentOnboardRequest()
            {
                StudentGeneralInfo = servetStudentGeneralInfoTM,
                ParentInfo = servetParentInfo,
                AcademicSummary = servetAcademicSummary,
                IsTryPracticeTYTExamBefore = true,
                LastPracticeTytExamPoints = validTytAveragePoints,
                IsTryPracticeAYTExamBefore = true,
                LastPracticeTmExamPoints = invalidTmAveragePoints,
                StudentGoals = emptyStudentGoals,
                SupplementaryMaterials = privateTutoringTytSupplementaryMaterials,
                ExpectationsFromCoaching = ":)"
            });
            
            yield return SwaggerExample.Create("Servet,Exam:Sozel,TYT:true,AYT:true,Goals:Empty,SupplementaryMaterials:privateTutoringTyt", new StudentOnboardRequest()
            {
                StudentGeneralInfo = servetStudentGeneralInfoSozel,
                ParentInfo = servetParentInfo,
                AcademicSummary = servetAcademicSummary,
                IsTryPracticeTYTExamBefore = true,
                LastPracticeTytExamPoints = validTytAveragePoints,
                IsTryPracticeAYTExamBefore = true,
                LastPracticeSozelExamPoints = validSozelAveragePoints,
                StudentGoals = emptyStudentGoals,
                SupplementaryMaterials = privateTutoringTytSupplementaryMaterials,
                ExpectationsFromCoaching = ":)"
            });
            
            yield return SwaggerExample.Create("Servet,Exam:Sozel,TYT:true,AYT:true-invalid,Goals:Empty,SupplementaryMaterials:privateTutoringTyt", new StudentOnboardRequest()
            {
                StudentGeneralInfo = servetStudentGeneralInfoSozel,
                ParentInfo = servetParentInfo,
                AcademicSummary = servetAcademicSummary,
                IsTryPracticeTYTExamBefore = true,
                LastPracticeTytExamPoints = validTytAveragePoints,
                IsTryPracticeAYTExamBefore = true,
                LastPracticeSozelExamPoints = invalidSozelAveragePoints,
                StudentGoals = emptyStudentGoals,
                SupplementaryMaterials = privateTutoringTytSupplementaryMaterials,
                ExpectationsFromCoaching = ":)"
            });
            
            yield return SwaggerExample.Create("Servet,Exam:Dil,TYT:true,AYT:true,Goals:Empty,SupplementaryMaterials:privateTutoringTyt", new StudentOnboardRequest()
            {
                StudentGeneralInfo = servetStudentGeneralInfoDil,
                ParentInfo = servetParentInfo,
                AcademicSummary = servetAcademicSummary,
                IsTryPracticeTYTExamBefore = true,
                LastPracticeTytExamPoints = validTytAveragePoints,
                IsTryPracticeAYTExamBefore = true,
                LastPracticeDilExamPoints = validDilAveragePoints,
                StudentGoals = emptyStudentGoals,
                SupplementaryMaterials = privateTutoringTytSupplementaryMaterials,
                ExpectationsFromCoaching = ":)"
            });
            
            yield return SwaggerExample.Create("Servet,Exam:Dil,TYT:true,AYT:true-invalid,Goals:Empty,SupplementaryMaterials:privateTutoringTyt", new StudentOnboardRequest()
            {
                StudentGeneralInfo = servetStudentGeneralInfoDil,
                ParentInfo = servetParentInfo,
                AcademicSummary = servetAcademicSummary,
                IsTryPracticeTYTExamBefore = true,
                LastPracticeTytExamPoints = validTytAveragePoints,
                IsTryPracticeAYTExamBefore = true,
                LastPracticeDilExamPoints = invalidDilAveragePoints,
                StudentGoals = emptyStudentGoals,
                SupplementaryMaterials = privateTutoringTytSupplementaryMaterials,
                ExpectationsFromCoaching = ":)"
            });
        }
    }
}
