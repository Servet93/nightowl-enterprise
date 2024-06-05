using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NightOwlEnterprise.Api.Entities;
using NightOwlEnterprise.Api.Entities.Enums;
using NightOwlEnterprise.Api.Utils;

namespace NightOwlEnterprise.Api.Endpoints.Students.Me;

public static class ManageInfo
{
    public static void MapManageInfo(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/me/info", async Task<Results<Ok<StudentProfileInfoResponse>, ProblemHttpResult>>
                (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                var dbContext = sp.GetRequiredService<ApplicationDbContext>();
                
                var paginationUriBuilder = sp.GetRequiredService<PaginationUriBuilder>();

                var studentId = claimsPrincipal.GetId();

                var user = dbContext.Users
                    .Include(x => x.SubscriptionHistories)
                    .Include(x => x.StudentDetail)
                    .FirstOrDefault(x => x.Id == studentId);
                
                var subscription = user.SubscriptionHistories.FirstOrDefault(x =>
                    x.SubscriptionEndDate != null && x.SubscriptionEndDate.Value > DateTime.UtcNow);
                
                var studentDetail = user.StudentDetail;
                
                var studentProfileInfoResponse =  new StudentProfileInfoResponse
                {
                    Email = user.Email!,
                    Status = studentDetail.Status,
                    Name = studentDetail.Name,
                    Surname = studentDetail.Surname,
                    Grade = GradeConverters.GetText(studentDetail.Grade),
                    ExamType = ExamTypeConverters.GetText(studentDetail.ExamType),
                    
                    SubscriptionType = subscription!.Type,
                    SubscriptionStartDate = subscription.SubscriptionStartDate,
                    SubscriptionEndDate = subscription.SubscriptionEndDate
                };
                
                var profilePhotoExist = dbContext.ProfilePhotos.Any(x => x.UserId == studentId);

                if (profilePhotoExist)
                {
                    studentProfileInfoResponse.ProfilePhotoUrl = paginationUriBuilder.GetStudentProfilePhotoUri(studentId);
                }
                
                var coachInfoResponse = dbContext.CoachStudentTrainingSchedules
                    .Include(x => x.Coach.CoachDetail)
                    .OrderByDescending(x => x.CreatedAt)
                    .Where(x => x.StudentId == studentId)
                    .Select(x => new
                    {
                        Name = x.Coach.CoachDetail.Name,
                        Surname = x.Coach.CoachDetail.Surname,
                    })
                    .FirstOrDefault();

                if (coachInfoResponse is not null)
                {
                    studentProfileInfoResponse.CoachName = coachInfoResponse.Name;
                    studentProfileInfoResponse.CoachSurname = coachInfoResponse.Surname;
                }

                return TypedResults.Ok(studentProfileInfoResponse);
            }).RequireAuthorization("Student").ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
            .WithTags(TagConstants.StudentsMeInfo);
    }

    public class StudentProfileInfoResponse
    {
        public string Name { get; set; }
        
        public string Surname { get; set; }
        public string Grade { get; set; }
        public string ExamType { get; set; }
        
        public string ProfilePhotoUrl { get; set; }
        
        public string CoachName { get; set; }
        
        public string CoachSurname { get; set; }
        
        public string Email { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StudentStatus Status { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SubscriptionType SubscriptionType { get; set; }
        public DateTime SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
    }
}