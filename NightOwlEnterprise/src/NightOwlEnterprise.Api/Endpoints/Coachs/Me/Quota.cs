using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace NightOwlEnterprise.Api.Endpoints.Coachs;

public static class Quota
{
    public static void MapQuota(this IEndpointRouteBuilder endpoints, CoachConfig coachConfig, PdrConfig pdrConfig)
    {
        endpoints.MapPost("/me/quota/",
                async Task<Results<Ok, ProblemHttpResult>>
                ([FromBody] CoachQuotaRequest request, ClaimsPrincipal claimsPrincipal,
                    [FromServices] IServiceProvider sp) =>
                {
                    var coachId = claimsPrincipal.GetId();

                    var totalQuota = request.Monday + request.Tuesday + request.Wednesday + request.Thursday +
                                     request.Friday + request.Saturday + request.Sunday;
                    
                    if (claimsPrincipal.IsInRole("Coach"))
                    {
                        if (totalQuota > coachConfig.MaxQuota || totalQuota < coachConfig.MinQuota)
                        {
                            return new ErrorDescriptor("CoachQuotaNotUpdated",
                                    $"Kontenjan min:{coachConfig.MinQuota}, max:{coachConfig.MaxQuota} olmalıdır")
                                .CreateProblem(
                                    "Koç kontenjan bilgisi güncellenemedi!");
                        }    
                    }else if (claimsPrincipal.IsInRole("Pdr"))
                    {
                        if (totalQuota > pdrConfig.MaxQuota || totalQuota < pdrConfig.MinQuota)
                        {
                            return new ErrorDescriptor("CoachQuotaNotUpdated",
                                    $"Kontenjan min:{pdrConfig.MinQuota}, max:{pdrConfig.MaxQuota} olmalıdır")
                                .CreateProblem(
                                    "Koç kontenjan bilgisi güncellenemedi!");
                        }
                    }

                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                    var coach = dbContext.CoachDetail.FirstOrDefault(x => x.CoachId == coachId);

                    coach.MondayQuota = request.Monday;
                    coach.TuesdayQuota = request.Tuesday;
                    coach.WednesdayQuota = request.Wednesday;
                    coach.ThursdayQuota = request.Thursday;
                    coach.FridayQuota = request.Friday;
                    coach.SaturdayQuota = request.Saturday;
                    coach.SundayQuota = request.Sunday;

                    await dbContext.SaveChangesAsync();
                    
                    return TypedResults.Ok();
                }).ProducesProblem(StatusCodes.Status400BadRequest).RequireAuthorization("CoachOrPdr").WithOpenApi()
            .WithTags(TagConstants.CoachMeQuota);
        
        endpoints.MapGet("/me/quota/",
                async Task<Results<Ok<CoachQuotaInfoResponse>, ProblemHttpResult>>
                (ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
                {
                    var coachId = claimsPrincipal.GetId();

                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                    var coachDetail = dbContext.CoachDetail.FirstOrDefault(x => x.CoachId == coachId);

                    var activeStudentCount = dbContext.CoachStudentTrainingSchedules.Count(x => x.CoachId == coachId);
                    
                    var response = new CoachQuotaInfoResponse
                    {
                        ActiveStudentCount = activeStudentCount,
                        Monday = coachDetail.MondayQuota ?? 0,
                        Tuesday = coachDetail.TuesdayQuota ?? 0,
                        Wednesday = coachDetail.WednesdayQuota ?? 0,
                        Thursday = coachDetail.ThursdayQuota ?? 0,
                        Friday = coachDetail.FridayQuota ?? 0,
                        Saturday = coachDetail.SaturdayQuota ?? 0,
                        Sunday = coachDetail.SundayQuota ?? 0
                    };

                    return TypedResults.Ok(response);
                    
                }).ProducesProblem(StatusCodes.Status400BadRequest).RequireAuthorization("CoachOrPdr").WithOpenApi()
            .WithTags(TagConstants.CoachMeQuota);
    }
    
    public sealed class CoachQuotaRequest
    {
        public required byte Monday { get; init; }
        public required byte Tuesday { get; init; }
        public required byte Wednesday { get; init; }
        public required byte Thursday { get; init; }
        public required byte Friday { get; init; }
        public required byte Saturday { get; init; }
        public required byte Sunday { get; init; }
    }
    
    public sealed class CoachQuotaInfoResponse
    {
        public int ActiveStudentCount { get; set; }
        public byte Monday { get; set; }
        public byte Tuesday { get; set; }
        public byte Wednesday { get; set; }
        public byte Thursday { get; set; }
        public byte Friday { get; set; }
        public byte Saturday { get; set; }
        public byte Sunday { get; set; }
    }
}