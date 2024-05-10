using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NightOwlEnterprise.Api.Services;

namespace NightOwlEnterprise.Api.Endpoints.Coachs;

public static class Quota
{
    public static void MapQuota(this IEndpointRouteBuilder endpoints, CoachConfig coachConfig)
    {
        endpoints.MapPost("/quota/",
            async Task<Results<Ok, ProblemHttpResult>>
                ([FromBody] CoachQuotaRequest request, ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
            {
                var totalQuota = request.Monday + request.Tuesday + request.Wednesday;

                if (totalQuota > coachConfig.MaxQuota || totalQuota < coachConfig.MinQuota)
                {
                    return new ErrorDescriptor("CoachQuotaNotUpdated", $"Kontenjan min:{coachConfig.MinQuota}, max:{coachConfig.MaxQuota} olmalıdır").CreateProblem(
                        "Koç kontenjan bilgisi güncellenemedi!");
                }
                
                var coachId = claimsPrincipal.GetId();
                
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

            }).ProducesProblem(StatusCodes.Status400BadRequest).RequireAuthorization("Coach").WithOpenApi().WithTags("Koç");
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
}