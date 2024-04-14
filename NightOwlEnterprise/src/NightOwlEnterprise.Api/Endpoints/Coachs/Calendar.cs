using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Swashbuckle.AspNetCore.Annotations;

namespace NightOwlEnterprise.Api.Endpoints.Coachs;

public static class Calendar
{
    public static void MapCalendar(this IEndpointRouteBuilder endpoints)
    {
        // NOTE: We cannot inject UserManager<TUser> directly because the TUser generic parameter is currently unsupported by RDG.
        // https://github.com/dotnet/aspnetcore/issues/47338
        endpoints.MapPost("/{coachId}/calendar/busy-day", async Task<Results<Ok<List<CalendarTime>>, ProblemHttpResult>>
            ([FromRoute] Guid coachId, HttpContext context, [FromServices] IServiceProvider sp) =>
        {
            var dbContext = sp.GetRequiredService<ApplicationDbContext>();

            var calendars = await dbContext.CoachCalendars.Where(x => x.CoachId == coachId && !x.IsAvailable).Select(x => new CalendarTime()            {
                Date = x.Date,
                StartTime = x.StartTime,
                EndTime = x.EndTime
            }).ToListAsync();

            return TypedResults.Ok(calendars);
            
        }).ProducesProblem(StatusCodes.Status400BadRequest).WithTags("Koç");
    }

    public sealed class CalendarTime
    {
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}