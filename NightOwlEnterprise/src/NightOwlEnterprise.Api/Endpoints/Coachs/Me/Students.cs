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
using NightOwlEnterprise.Api.Endpoints.CommonDto;
using NightOwlEnterprise.Api.Endpoints.Students;
using NightOwlEnterprise.Api.Entities;
using NightOwlEnterprise.Api.Entities.Enums;
using NightOwlEnterprise.Api.Utils;
using Stripe;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using ResourcesAYT = NightOwlEnterprise.Api.Entities.ResourcesAYT;
using ResourcesTYT = NightOwlEnterprise.Api.Entities.ResourcesTYT;

namespace NightOwlEnterprise.Api.Endpoints.Coachs.Me;

public static class Students
{
    public static void MapStudents(this IEndpointRouteBuilder endpoints)
    {
        // NOTE: We cannot inject UserManager<TUser> directly because the TUser generic parameter is currently unsupported by RDG.
        // https://github.com/dotnet/aspnetcore/issues/47338
        endpoints.MapPost("/me/students", async Task<Results<Ok<PagedResponse<StudentItem>>, ProblemHttpResult>>
            ([FromBody] StudentFilterRequest? filter, [FromQuery] int? page,[FromQuery] int? pageSize, ClaimsPrincipal claimsPrincipal, HttpContext httpContext, [FromServices] IServiceProvider sp) =>
        {
            var paginationFilter = new PaginationFilter(page, pageSize);
            
            var dbContext = sp.GetRequiredService<ApplicationDbContext>();
            
            var paginationUriBuilder = sp.GetRequiredService<PaginationUriBuilder>();
        
            var coachId = claimsPrincipal.GetId();

            var studentOfCoachQueryable = dbContext.CoachStudentTrainingSchedules.Where(x => x.CoachId == coachId);
                
            var totalCount = await studentOfCoachQueryable.CountAsync();

            studentOfCoachQueryable = studentOfCoachQueryable
                .Include(x => x.Student)
                .Include(x => x.Student.StudentDetail);
            
            if (filter is not null)
            {
                if (filter.Grade is not null)
                {
                    var gradeFilterItems = new List<Grade>();
                    
                    if (filter.Grade.Dokuz.HasValue && filter.Grade.Dokuz.Value)
                        gradeFilterItems.Add(Grade.Dokuz);
                    
                    if (filter.Grade.On.HasValue && filter.Grade.On.Value)
                        gradeFilterItems.Add(Grade.On);
                    
                    if (filter.Grade.Onbir.HasValue && filter.Grade.Onbir.Value)
                        gradeFilterItems.Add(Grade.Onbir);
                    
                    if (filter.Grade.Oniki.HasValue && filter.Grade.Oniki.Value)
                        gradeFilterItems.Add(Grade.Oniki);
                    
                    if (filter.Grade.Mezun.HasValue && filter.Grade.Mezun.Value)
                        gradeFilterItems.Add(Grade.Mezun);

                    studentOfCoachQueryable = studentOfCoachQueryable.WhereIf(x => gradeFilterItems.Contains(x.Student.StudentDetail.Grade),
                        gradeFilterItems.Any());
                }

                if (filter.Video is not null)
                {
                    var videoReservationDayFilterItems = new List<DayOfWeek>();
                    
                    if (filter.Video.Monday.HasValue && filter.Video.Monday.Value)
                        videoReservationDayFilterItems.Add(DayOfWeek.Monday);
                    
                    if (filter.Video.Tuesday.HasValue && filter.Video.Tuesday.Value)
                        videoReservationDayFilterItems.Add(DayOfWeek.Tuesday);
                    
                    if (filter.Video.Wednesday.HasValue && filter.Video.Wednesday.Value)
                        videoReservationDayFilterItems.Add(DayOfWeek.Wednesday);

                    if (filter.Video.Thursday.HasValue && filter.Video.Thursday.Value)
                        videoReservationDayFilterItems.Add(DayOfWeek.Thursday);
                    
                    if (filter.Video.Friday.HasValue && filter.Video.Friday.Value)
                        videoReservationDayFilterItems.Add(DayOfWeek.Friday);
                    
                    if (filter.Video.Saturday.HasValue && filter.Video.Saturday.Value)
                        videoReservationDayFilterItems.Add(DayOfWeek.Saturday);
                    
                    if (filter.Video.Sunday.HasValue && filter.Video.Sunday.Value)
                        videoReservationDayFilterItems.Add(DayOfWeek.Sunday);
                    
                    studentOfCoachQueryable = studentOfCoachQueryable.WhereIf(
                        x => x.VideoDay.HasValue && videoReservationDayFilterItems.Contains(x.VideoDay.Value),
                        videoReservationDayFilterItems.Any());
                }
                
                if (filter.Voice is not null)
                {
                    var voiceReservationDayFilterItems = new List<DayOfWeek>();
                    
                    if (filter.Voice.Monday.HasValue && filter.Voice.Monday.Value)
                        voiceReservationDayFilterItems.Add(DayOfWeek.Monday);
                    
                    if (filter.Voice.Tuesday.HasValue && filter.Voice.Tuesday.Value)
                        voiceReservationDayFilterItems.Add(DayOfWeek.Tuesday);
                    
                    if (filter.Voice.Wednesday.HasValue && filter.Voice.Wednesday.Value)
                        voiceReservationDayFilterItems.Add(DayOfWeek.Wednesday);

                    if (filter.Voice.Thursday.HasValue && filter.Voice.Thursday.Value)
                        voiceReservationDayFilterItems.Add(DayOfWeek.Thursday);
                    
                    if (filter.Voice.Friday.HasValue && filter.Voice.Friday.Value)
                        voiceReservationDayFilterItems.Add(DayOfWeek.Friday);
                    
                    if (filter.Voice.Saturday.HasValue && filter.Voice.Saturday.Value)
                        voiceReservationDayFilterItems.Add(DayOfWeek.Saturday);
                    
                    if (filter.Voice.Sunday.HasValue && filter.Voice.Sunday.Value)
                        voiceReservationDayFilterItems.Add(DayOfWeek.Sunday);
                    
                    studentOfCoachQueryable = studentOfCoachQueryable.WhereIf(
                        x => x.VoiceDay.HasValue && voiceReservationDayFilterItems.Contains(x.VoiceDay.Value),
                        voiceReservationDayFilterItems.Any());
                }

                if (filter.ExamFilter is not null)
                {
                    var examFilterItems = new List<ExamType>();
                    
                    if (filter.ExamFilter.TYT_TM.HasValue && filter.ExamFilter.TYT_TM.Value)
                        examFilterItems.Add(ExamType.TYT_TM);
                    
                    if (filter.ExamFilter.TYT_MF.HasValue && filter.ExamFilter.TYT_MF.Value)
                        examFilterItems.Add(ExamType.TYT_MF);
                    
                    if (filter.ExamFilter.TYT_SOZEL.HasValue && filter.ExamFilter.TYT_SOZEL.Value)
                        examFilterItems.Add(ExamType.TYT_SOZEL);
                    
                    if (filter.ExamFilter.TM.HasValue && filter.ExamFilter.TM.Value)
                        examFilterItems.Add(ExamType.TM);
                    
                    if (filter.ExamFilter.MF.HasValue && filter.ExamFilter.MF.Value)
                        examFilterItems.Add(ExamType.MF);
                    
                    if (filter.ExamFilter.Sozel.HasValue && filter.ExamFilter.Sozel.Value)
                        examFilterItems.Add(ExamType.Sozel);
                    
                    if (filter.ExamFilter.Dil.HasValue && filter.ExamFilter.Dil.Value)
                        examFilterItems.Add(ExamType.Dil);
                    
                    studentOfCoachQueryable = studentOfCoachQueryable.WhereIf(x => examFilterItems.Contains(x.Student.StudentDetail.ExamType),
                        examFilterItems.Any());
                }
            }

            var studentsOfCoach = await studentOfCoachQueryable
                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize).ToListAsync();
            
            var students = new List<StudentItem>();
            
            var studentProfilePhotos = new List<Guid>();

            if (studentsOfCoach.Any())
            {
                var studentIds = studentsOfCoach.Select(x => x.StudentId);
                
                studentProfilePhotos = dbContext.ProfilePhotos.Where(x => studentIds.Contains(x.UserId)).Select(x => x.UserId).ToList();
            }
            
            foreach (var studentOfCoach in studentsOfCoach)
            {
                var studentDetail = studentOfCoach.Student.StudentDetail;
                
                var studentItem = new StudentItem()
                {
                    Id = studentOfCoach.StudentId,
                    Name = studentDetail.Name,
                    Surname = studentDetail.Surname,
                    Highschool = studentDetail.HighSchool,
                    Grade = studentOfCoach.Student.StudentDetail.Grade,
                    GradeText = GradeConverters.GetText(studentDetail.Grade),
                    ExamType = studentOfCoach.Student.StudentDetail.ExamType,
                    ExamTypeText = ExamTypeConverters.GetText(studentDetail.ExamType),
                };

                if (studentProfilePhotos.Contains(studentOfCoach.StudentId))
                {
                    studentItem.ProfilePhotoUrl =
                        paginationUriBuilder.GetStudentProfilePhotoUri(studentOfCoach.StudentId);
                }
                
                students.Add(studentItem);
            }
                
            var pagedResponse = PagedResponse<StudentItem>.CreatePagedResponse(
                students, totalCount, paginationFilter, paginationUriBuilder,
                httpContext.Request.Path.Value ?? string.Empty);
        
            return TypedResults.Ok(pagedResponse);
        
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachMeStudents).RequireAuthorization("CoachOrPdr");
        
        endpoints.MapGet("me/students/{studentId}", async Task<Results<Ok<StudentItem>, ProblemHttpResult>>
            ([FromQuery] Guid studentId, ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
        {
            var dbContext = sp.GetRequiredService<ApplicationDbContext>();
            
            var paginationUriBuilder = sp.GetRequiredService<PaginationUriBuilder>();
            
            var coachId = claimsPrincipal.GetId();
            
            var hasCoachStudentTrainingSchedule = dbContext.CoachStudentTrainingSchedules
                .Include(x => x.Student)
                .Include(x => x.Student.StudentDetail)
                .FirstOrDefault(x => x.CoachId == coachId && x.StudentId == studentId);
        
            if (hasCoachStudentTrainingSchedule is null)
            {
                return new ErrorDescriptor("CoachHasNotStudent", "Koç ilgili öğrenci ile çalışmamaktadır.").CreateProblem("Öğrenci Bilgisi Getirilemedi");
            }

            var student = hasCoachStudentTrainingSchedule.Student;

            var studentDetail = student.StudentDetail;
            
            var studentItem = new StudentItem()
            {
                Id = studentId,
                Name = studentDetail.Name,
                Surname = studentDetail.Surname,
                Highschool = studentDetail.HighSchool,
                Grade = student.StudentDetail.Grade,
                ExamType = student.StudentDetail.ExamType,
                GradeText = GradeConverters.GetText(studentDetail.Grade),
                ExamTypeText = ExamTypeConverters.GetText(studentDetail.ExamType) ,
            };

            var isExist = dbContext.ProfilePhotos.Any(x => x.UserId == studentId);

            if (isExist)
            {
                studentItem.ProfilePhotoUrl = paginationUriBuilder.GetStudentProfilePhotoUri(studentId);
            }
        
            return TypedResults.Ok(studentItem);
            
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachMeStudents).RequireAuthorization("CoachOrPdr");
        
        endpoints.MapGet("me/students/{studentId}/onboard-info", async Task<Results<Ok<StudentOnboardInfo>, ProblemHttpResult>>
            ([FromQuery] Guid studentId, ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
        {
            var dbContext = sp.GetRequiredService<ApplicationDbContext>();
            
            var coachId = claimsPrincipal.GetId();

            var hasCoachStudentTrainingSchedule = dbContext.CoachStudentTrainingSchedules
                .Include(x => x.Student)
                .Include(x => x.Student.StudentDetail)
                .Include(x => x.Student.TytNets)
                .Include(x => x.Student.TmNets)
                .Include(x => x.Student.MfNets)
                .Include(x => x.Student.SozelNets)
                .Include(x => x.Student.DilNets)
                .Include(x => x.Student.PrivateTutoringTM)
                .Include(x => x.Student.PrivateTutoringMF)
                .Include(x => x.Student.PrivateTutoringTYT)
                .Include(x => x.Student.PrivateTutoringDil)
                .Include(x => x.Student.PrivateTutoringSozel)
                .FirstOrDefault(x => x.CoachId == coachId && x.StudentId == studentId);
        
            if (hasCoachStudentTrainingSchedule is null)
            {
                return new ErrorDescriptor("CoachHasNotStudent", "Koç ilgili öğrenci ile çalışmamaktadır.").CreateProblem("Öğrenci Bilgisi Getirilemedi");
            }

            var student = hasCoachStudentTrainingSchedule.Student;
            var studentDetail = student.StudentDetail;
            
            var studentOnboardInfo = new StudentOnboardInfo()
            {
                StudentGeneralInfo = new StudentGeneralInfo()
                {
                    Name = studentDetail.Name,
                    Surname = studentDetail.Surname,
                    Email = studentDetail.Email,
                    Mobile = studentDetail.Mobile,
                    Grade = studentDetail.Grade,
                    ExamType = studentDetail.ExamType
                },
                ParentInfo = new ParentInfo()
                {
                    Name = studentDetail.ParentName,
                    Surname = studentDetail.ParentSurname,
                    Email = studentDetail.Email,
                    Mobile = studentDetail.Mobile,
                },
                AcademicSummary = new AcademicSummary()
                {
                    HighSchool = studentDetail.HighSchool,
                    HighSchoolGPA = studentDetail.HighSchoolGPA.Value
                },
                StudentGoals = new StudentGoals()
                {
                    TytGoalNet = studentDetail.TytGoalNet,
                    AytGoalNet = studentDetail.AytGoalNet,
                    GoalRanking = studentDetail.GoalRanking,
                    DesiredProfessionSchoolField = studentDetail.DesiredProfessionSchoolField
                },
                ExpectationsFromCoaching = studentDetail.ExpectationsFromCoaching,
                SupplementaryMaterials = new SupplementaryMaterials()
                {
                    Course = studentDetail.Course ?? false,
                    School = studentDetail.School  ?? false,
                    Youtube = studentDetail.Youtube ?? false,
                }
            };

            if (student.TytNets is not null)
            {
                studentOnboardInfo.IsTryPracticeTYTExamBefore = true;
                studentOnboardInfo.LastPracticeTytExamPoints = new StudentTytNets()
                {
                    Biology = student.TytNets.Biology ?? 0,
                    Chemistry = student.TytNets.Chemistry ?? 0,
                    Geography = student.TytNets.Geography ?? 0,
                    Geometry = student.TytNets.Geometry ?? 0,
                    History = student.TytNets.History.Value,
                    Mathematics = student.TytNets.Mathematics.Value,
                    Philosophy = student.TytNets.Philosophy.Value,
                    Physics = student.TytNets.Physics.Value,
                    Religion = student.TytNets.Religion.Value,
                    Turkish = student.TytNets.Turkish.Value,
                };
            }
            
            if (studentDetail.ExamType == ExamType.TM && student.TmNets is not null)
            {
                studentOnboardInfo.IsTryPracticeAYTExamBefore = true;            
                studentOnboardInfo.LastPracticeTmExamPoints = new StudentTmNets()
                {
                    Geography = student.TmNets.Geography.Value,
                    Geometry = student.TmNets.Geometry.Value,
                    History = student.TmNets.History.Value,
                    Literature = student.TmNets.Literature.Value,
                    Mathematics = student.TmNets.Mathematics.Value,
                };
            }
            else if (studentDetail.ExamType == ExamType.MF && student.MfNets is not null)
            {
                studentOnboardInfo.IsTryPracticeAYTExamBefore = true;
                studentOnboardInfo.LastPracticeMfExamPoints = new StudentMfNets()
                {
                    Biology = student.MfNets.Biology.Value,
                    Chemistry = student.MfNets.Chemistry.Value,
                    Geometry = student.MfNets.Geometry.Value,
                    Mathematics = student.MfNets.Mathematics.Value,
                    Physics = student.MfNets.Physics.Value,
                };
            }
            else if (studentDetail.ExamType == ExamType.Sozel && student.SozelNets is not null)
            {
                studentOnboardInfo.IsTryPracticeAYTExamBefore = true;
                studentOnboardInfo.LastPracticeSozelExamPoints = new StudentSozelNets()
                {
                    Geography1 = student.SozelNets.Geography1.Value,
                    Geography2 = student.SozelNets.Geography2.Value,
                    History1 = student.SozelNets.History1.Value,
                    History2 = student.SozelNets.History2.Value,
                    Literature1 = student.SozelNets.Literature1.Value,
                    Philosophy = student.SozelNets.Philosophy.Value,
                    Religion = student.SozelNets.Religion.Value,
                };
            }
            else if (studentDetail.ExamType == ExamType.Dil && student.DilNets is not null)
            {
                studentOnboardInfo.IsTryPracticeAYTExamBefore = true;
                studentOnboardInfo.LastPracticeDilExamPoints = new StudentDilNets()
                {
                    YDT = student.DilNets.YDT.Value
                };
            }

            if (student.ResourcesTYT is not null)
            {
                student.ResourcesTYT = new ResourcesTYT()
                {
                    Biology = student.ResourcesTYT.Biology,
                    Chemistry = student.ResourcesTYT.Chemistry,
                    Geography = student.ResourcesTYT.Geography,
                    Geometry = student.ResourcesTYT.Geometry,
                    History = student.ResourcesTYT.History,
                    Mathematics = student.ResourcesTYT.Mathematics,
                    Philosophy = student.ResourcesTYT.Philosophy,
                    Physics = student.ResourcesTYT.Physics,
                    Religion = student.ResourcesTYT.Religion,
                    Turkish = student.ResourcesTYT.Turkish,
                };
            }
            
            if (student.ResourcesAYT is not null)
            {
                student.ResourcesAYT = new ResourcesAYT()
                {
                    Biology = student.ResourcesAYT.Biology,
                    Chemistry = student.ResourcesAYT.Chemistry,
                    Geography = student.ResourcesAYT.Geography,
                    Geometry = student.ResourcesAYT.Geometry,
                    History = student.ResourcesAYT.History,
                    Mathematics = student.ResourcesAYT.Mathematics,
                    Philosophy = student.ResourcesAYT.Philosophy,
                    Physics = student.ResourcesAYT.Physics,
                    Religion = student.ResourcesAYT.Religion,
                    Turkish = student.ResourcesAYT.Turkish,
                };
            }

            if (student.PrivateTutoringTYT is not null)
            {
                studentOnboardInfo.SupplementaryMaterials.PrivateTutoringTyt = true;
                studentOnboardInfo.SupplementaryMaterials.PrivateTutoringTytLessons = new PrivateTutoringTYTObject()
                {
                    Biology = student.PrivateTutoringTYT.Biology,
                    Chemistry = student.PrivateTutoringTYT.Chemistry,
                    Geography = student.PrivateTutoringTYT.Geography,
                    Geometry = student.PrivateTutoringTYT.Geometry,
                    History = student.PrivateTutoringTYT.History,
                    Mathematics = student.PrivateTutoringTYT.Mathematics,
                    Philosophy = student.PrivateTutoringTYT.Philosophy,
                    Physics = student.PrivateTutoringTYT.Physics,
                    Religion = student.PrivateTutoringTYT.Religion,
                    Turkish = student.PrivateTutoringTYT.Turkish,
                };
            }

            if (studentDetail.ExamType == ExamType.TM && student.PrivateTutoringTM is not null)
            {
                studentOnboardInfo.SupplementaryMaterials.PrivateTutoringAyt = true;
                studentOnboardInfo.SupplementaryMaterials.PrivateTutoringAytLessons = new PrivateTutoringAYTObject()
                {
                    Tm = new TM()
                    {
                        Geography = student.PrivateTutoringTM.Geography,
                        Geometry = student.PrivateTutoringTM.Geometry,
                        History = student.PrivateTutoringTM.History,
                        Literature = student.PrivateTutoringTM.Literature,
                        Mathematics = student.PrivateTutoringTM.Mathematics,
                    }
                };
            }
            else if (studentDetail.ExamType == ExamType.MF && student.PrivateTutoringMF is not null)
            {
                studentOnboardInfo.SupplementaryMaterials.PrivateTutoringAyt = true;
                studentOnboardInfo.SupplementaryMaterials.PrivateTutoringAytLessons = new PrivateTutoringAYTObject()
                {
                    Mf = new MF()
                    {
                        Biology = student.PrivateTutoringMF.Biology,
                        Chemistry = student.PrivateTutoringMF.Chemistry,
                        Geometry = student.PrivateTutoringMF.Geometry,
                        Mathematics = student.PrivateTutoringMF.Mathematics,
                        Physics = student.PrivateTutoringMF.Physics,
                    } 
                };
            }
            else if (studentDetail.ExamType == ExamType.Sozel && student.PrivateTutoringSozel is not null)
            {
                studentOnboardInfo.SupplementaryMaterials.PrivateTutoringAyt = true;
                studentOnboardInfo.SupplementaryMaterials.PrivateTutoringAytLessons = new PrivateTutoringAYTObject()
                {
                    Sozel = new Sozel()
                    {
                        Geography1 = student.PrivateTutoringSozel.Geography1,
                        Geography2 = student.PrivateTutoringSozel.Geography2,
                        History1 = student.PrivateTutoringSozel.History1,
                        History2 = student.PrivateTutoringSozel.History2,
                        Literature1 = student.PrivateTutoringSozel.Literature1,
                        Philosophy = student.PrivateTutoringSozel.Philosophy,
                        Religion = student.PrivateTutoringSozel.Religion,
                    } 
                };
            }
            else if (studentDetail.ExamType == ExamType.Dil && student.PrivateTutoringDil is not null)
            {
                studentOnboardInfo.SupplementaryMaterials.PrivateTutoringAyt = true;
                studentOnboardInfo.SupplementaryMaterials.PrivateTutoringAytLessons = new PrivateTutoringAYTObject()
                {
                    Dil = new Dil()
                    {
                        YTD = student.PrivateTutoringDil.YTD,
                    } 
                };
            }
        
            return TypedResults.Ok(studentOnboardInfo);
            
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachMeStudents).RequireAuthorization("CoachOrPdr");
        
         endpoints.MapPost("/me/students/invitation-day-info", async Task<Results<Ok<PagedResponse<StudentInvitationDayInfo>>, ProblemHttpResult>>
            ([FromQuery] int? page,[FromQuery] int? pageSize, ClaimsPrincipal claimsPrincipal, HttpContext httpContext, [FromServices] IServiceProvider sp) =>
        {
            var paginationFilter = new PaginationFilter(page, pageSize);
            
            var dbContext = sp.GetRequiredService<ApplicationDbContext>();
            
            var paginationUriBuilder = sp.GetRequiredService<PaginationUriBuilder>();
        
            var coachId = claimsPrincipal.GetId();

            var studentOfCoachQueryable = dbContext.CoachStudentTrainingSchedules.Where(x => x.CoachId == coachId);
                
            var totalCount = await studentOfCoachQueryable.CountAsync();

            var studentsOfCoach = await studentOfCoachQueryable
                .Include(x => x.Student)
                .Include(x => x.Student.StudentDetail)
                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize).ToListAsync();
            
            var students = new List<StudentInvitationDayInfo>();
            
            var studentProfilePhotos = new List<Guid>();

            if (studentsOfCoach.Any())
            {
                var studentIds = studentsOfCoach.Select(x => x.StudentId);
                
                studentProfilePhotos = dbContext.ProfilePhotos.Where(x => studentIds.Contains(x.UserId)).Select(x => x.UserId).ToList();
            }

            foreach (var studentOfCoach in studentsOfCoach)
            {
                var studentItem = new StudentInvitationDayInfo()
                {
                    Id = studentOfCoach.StudentId,
                    Name = studentOfCoach.Student.StudentDetail.Name,
                    Surname = studentOfCoach.Student.StudentDetail.Surname,
                    Day = studentOfCoach.VideoDay
                };

                if (studentProfilePhotos.Contains(studentOfCoach.StudentId))
                {
                    studentItem.ProfilePhotoUrl =
                        paginationUriBuilder.GetStudentProfilePhotoUri(studentOfCoach.StudentId);
                }
                
                students.Add(studentItem);
            }
                
            var pagedResponse = PagedResponse<StudentInvitationDayInfo>.CreatePagedResponse(
                students, totalCount, paginationFilter, paginationUriBuilder,
                httpContext.Request.Path.Value ?? string.Empty);
        
            return TypedResults.Ok(pagedResponse);
        
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags(TagConstants.CoachMeStudents).RequireAuthorization("CoachOrPdr");
    }
    
    public sealed class StudentFilterRequest
    {
        public GradeFilter? Grade { get; set; }
        
        public ExamFilter ExamFilter { get; set; }

        public ReservationDayFilter? Video { get; set; }
        
        public ReservationDayFilter? Voice { get; set; }
    }
    
    public sealed class GradeFilter
    {
        public bool? Dokuz { get; set; }
        public bool? On { get; set; }
        public bool? Onbir { get; set; }
        public bool? Oniki { get; set; }
        public bool? Mezun { get; set; }
    }
    
    public sealed class ExamFilter
    {
        public bool? TYT_TM { get; set; }
        public bool? TYT_MF { get; set; }
        public bool? TYT_SOZEL { get; set; }
        public bool? TM { get; set; }
        public bool? MF { get; set; }
        public bool? Sozel { get; set; }
        public bool? Dil { get; set; }
    }
    
    public sealed class ReservationDayFilter
    {
        public bool? Sunday { get; set; }
        public bool? Monday { get; set; }
        public bool? Tuesday { get; set; }
        public bool? Wednesday { get; set; }
        public bool? Thursday { get; set; }
        public bool? Friday { get; set; }
        public bool? Saturday { get; set; }
    }
    
    public sealed class StudentItem
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public string Surname { get; set; }
        public string Highschool { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Grade Grade { get; set; }
        
        public string GradeText { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ExamType ExamType { get; set; }
        
        public string ExamTypeText { get; set; }
        public string? ProfilePhotoUrl { get; set; }
    }

    public sealed class StudentInvitationDayInfo
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public string Surname { get; set; }
        
        public string? ProfilePhotoUrl { get; set; }
        
        public DayOfWeek? Day { get; set; }
    }
    
    public class StudentOnboardInfo
    {
        public StudentGeneralInfo? StudentGeneralInfo { get; set; }
        
        public ParentInfo? ParentInfo { get; set; }
        
        public AcademicSummary? AcademicSummary { get; set; }
        
        //Daha önce TYT denemesine girdiniz mi? True ise LastPracticeTytExamPoints dolu olmalı. False ise 
        public bool IsTryPracticeTYTExamBefore { get; set; }
        
        public StudentTytNets? LastPracticeTytExamPoints { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise
        //LastPracticeTytExamPoints dolu olmalı. False ise 
        public bool IsTryPracticeAYTExamBefore { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise && ExamType -> MF
        public StudentMfNets? LastPracticeMfExamPoints { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise && ExamType -> TM
        public StudentTmNets? LastPracticeTmExamPoints { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise && ExamType -> Sozel
        public StudentSozelNets? LastPracticeSozelExamPoints { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise && ExamType -> Dil
        public StudentDilNets? LastPracticeDilExamPoints { get; set; }
        
        public StudentGoals? StudentGoals { get; set; }
        
        public ResourcesTYT? ResourcesTYT { get; set; }
        
        public ResourcesAYT? ResourcesAYT { get; set; }
        
        public SupplementaryMaterials? SupplementaryMaterials { get; set; }
    
        //Koçluktan beklentin nedir? (Free text uzun paragraf)
        public string ExpectationsFromCoaching { get; set; }
    }
}