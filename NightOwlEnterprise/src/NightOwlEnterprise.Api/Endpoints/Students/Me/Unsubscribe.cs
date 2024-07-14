using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NightOwlEnterprise.Api.Entities;
using NightOwlEnterprise.Api.Entities.Enums;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using Stripe;

namespace NightOwlEnterprise.Api.Endpoints.Students.Me;

public static class Unsubscribe
{
    public static void MapUnsubscribe(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/me/unsubscribe", async Task<Results<Ok, ProblemHttpResult>>
                (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                var logger = loggerFactory.CreateLogger("Student-Unsubscribe");
                
                var studentId = claimsPrincipal.GetId();

                var dbContext = sp.GetService<ApplicationDbContext>();

                var student = dbContext.Users.FirstOrDefault(x => x.Id == studentId);

                if (student is null)
                {
                    logger.LogWarning("Student Not Found. StudentId: {StudentId}", studentId);
                    
                    var errorDescriptor = new ErrorDescriptor("StudentNotFound", "Öğrenci bulunamadı!");

                    return errorDescriptor.CreateProblem("Abonelik iptali başarısız!");
                }
                
                var subscriptionService = new SubscriptionService();

                var cancelOptions = new SubscriptionCancelOptions
                {
                    // İptalin fatura dönemi sonunda mı yoksa hemen mi olacağını belirler
                    Prorate = true // true olarak ayarlarsanız hemen iptal eder ve kalan dönemin ücretini iade eder
                };
                
                try
                {
                    // Aboneliği iptal et
                    Subscription subscription = subscriptionService.Cancel(student.SubscriptionId, cancelOptions);
                    
                    logger.LogInformation(
                        "Subscription has been cancelled. StudentId: {StudentId}, SubscriptionId: {SubscriptionId}",
                        studentId, student.SubscriptionId);
                }
                catch (StripeException e)
                {
                    logger.LogError(e,
                        "Subscription Cancel Error. StudentId: {StudentId}, SubscriptionId: {SubscriptionId}", studentId,
                        student.SubscriptionId);
                    
                    var errorDescriptor = new ErrorDescriptor("SubscriptionCancelFailed", $"Servis hatası -> {e.Message}");

                    return errorDescriptor.CreateProblem("Abonelik iptali başarısız!");
                }

                dbContext.Users.Remove(student);

                try
                {
                    dbContext.SaveChanges();
                }
                catch (Exception e)
                {
                    logger.LogError(e,
                        "Student Couldn't remove from db. StudentId: {StudentId}, SubscriptionId: {SubscriptionId}", studentId,
                        student.SubscriptionId);
                    
                    var errorDescriptor = new ErrorDescriptor("UserCouldNotRemove", "Öğrenci kaydı silinemedi!");

                    return errorDescriptor.CreateProblem("Abonelik iptal edildi fakat öğrenci kaydı silinemedi!");
                }
                
                return TypedResults.Ok();
                
            }).RequireAuthorization("Student").ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi()
            .WithTags(TagConstants.StudentsMeInfo);
    }
}