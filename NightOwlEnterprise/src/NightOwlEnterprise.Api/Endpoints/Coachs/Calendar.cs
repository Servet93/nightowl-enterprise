using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NightOwlEnterprise.Api.Services;

namespace NightOwlEnterprise.Api.Endpoints.Coachs;

public static class Calendar
{
    public static void MapCalendar(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/{coachId}/calendar/",
            async Task<Results<Ok<DaysAvailability>, ProblemHttpResult>>
                ([FromRoute] Guid coachId, HttpContext context, [FromServices] IServiceProvider sp) =>
            {
                var dbContext = sp.GetRequiredService<ApplicationDbContext>();
                
                var getCoachAvailabilityDays = sp.GetRequiredService<GetCoachAvailabilityDays>();

                var daysAvailability = await getCoachAvailabilityDays.GetAsync(dbContext, coachId);

                return TypedResults.Ok(daysAvailability);

            }).ProducesProblem(StatusCodes.Status400BadRequest).WithOpenApi().WithTags("Öğrencinin Koç ile yapabileceği işlemler");
    }
}