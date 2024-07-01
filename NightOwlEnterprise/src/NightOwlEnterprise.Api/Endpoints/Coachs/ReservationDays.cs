using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NightOwlEnterprise.Api.Utils;

namespace NightOwlEnterprise.Api.Endpoints.Coachs;

public static class ReservationDays
{
    public static void MapReservationDays(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/{coachId}/reservation-days/",
                async Task<Results<Ok<DaysAvailability>, ProblemHttpResult>>
                    ([FromRoute] Guid coachId, [FromServices] IServiceProvider sp) =>
                {
                    var dbContext = sp.GetRequiredService<ApplicationDbContext>();

                    var dayToStudentCount = dbContext.CoachStudentTrainingSchedules
                        .Where(x => x.CoachId == coachId)
                        .GroupBy(x => x.VideoDay)
                        .ToDictionary(x => x.Key, x => (byte)x.Count());

                    var coachDaysQuota = await dbContext.CoachDetail.Where(x => x.CoachId == coachId).Select(x =>
                        new CoachDaysQuota()
                        {
                            MondayQuota = x.MondayQuota.Value,
                            TuesdayQuota = x.TuesdayQuota.Value,
                            WednesdayQuota = x.WednesdayQuota.Value,
                            ThursdayQuota = x.ThursdayQuota.Value,
                            FridayQuota = x.FridayQuota.Value,
                            SaturdayQuota = x.SaturdayQuota.Value,
                            SundayQuota = x.SundayQuota.Value,
                        }).FirstOrDefaultAsync();

                    var mondayStudentCount = dayToStudentCount.ContainsKey(DayOfWeek.Monday)
                        ? dayToStudentCount[DayOfWeek.Monday]
                        : 0;

                    var mondayRemainQuota = coachDaysQuota.MondayQuota > mondayStudentCount
                        ? coachDaysQuota.MondayQuota - mondayStudentCount
                        : 0;

                    var tuesdayStudentCount = dayToStudentCount.ContainsKey(DayOfWeek.Tuesday)
                        ? dayToStudentCount[DayOfWeek.Tuesday]
                        : 0;

                    var tuesdayRemainQuota = coachDaysQuota.TuesdayQuota > tuesdayStudentCount
                        ? coachDaysQuota.TuesdayQuota - tuesdayStudentCount
                        : 0;

                    var wednesdayStudentCount = dayToStudentCount.ContainsKey(DayOfWeek.Wednesday)
                        ? dayToStudentCount[DayOfWeek.Wednesday]
                        : 0;

                    var wednesdayRemainQuota = coachDaysQuota.WednesdayQuota > wednesdayStudentCount
                        ? coachDaysQuota.WednesdayQuota - wednesdayStudentCount
                        : 0;

                    var thursdayStudentCount = dayToStudentCount.ContainsKey(DayOfWeek.Thursday)
                        ? dayToStudentCount[DayOfWeek.Thursday]
                        : 0;

                    var thursdayRemainQuota = coachDaysQuota.ThursdayQuota > thursdayStudentCount
                        ? coachDaysQuota.ThursdayQuota - thursdayStudentCount
                        : 0;

                    var fridayStudentCount = dayToStudentCount.ContainsKey(DayOfWeek.Friday)
                        ? dayToStudentCount[DayOfWeek.Friday]
                        : 0;

                    var fridayRemainQuota = coachDaysQuota.FridayQuota > fridayStudentCount
                        ? coachDaysQuota.FridayQuota - fridayStudentCount
                        : 0;

                    var saturdayStudentCount = dayToStudentCount.ContainsKey(DayOfWeek.Saturday)
                        ? dayToStudentCount[DayOfWeek.Saturday]
                        : 0;

                    var saturdayRemainQuota = coachDaysQuota.SaturdayQuota > saturdayStudentCount
                        ? coachDaysQuota.SaturdayQuota - saturdayStudentCount
                        : 0;

                    var sundayStudentCount = dayToStudentCount.ContainsKey(DayOfWeek.Sunday)
                        ? dayToStudentCount[DayOfWeek.Sunday]
                        : 0;

                    var sundayRemainQuota = coachDaysQuota.SundayQuota > sundayStudentCount
                        ? coachDaysQuota.SundayQuota - sundayStudentCount
                        : 0;

                    var daysAvailability = new DaysAvailability()
                    {
                        Monday =
                        {
                            TotalQuota = coachDaysQuota.MondayQuota, RemainQuota = mondayRemainQuota,
                            IsAbleToReserve = mondayRemainQuota > 0,
                            Date = DateUtils.FindDate(DayOfWeek.Monday)
                        },
                        Tuesday =
                        {
                            TotalQuota = coachDaysQuota.TuesdayQuota, RemainQuota = tuesdayRemainQuota,
                            IsAbleToReserve = tuesdayRemainQuota > 0,
                            Date = DateUtils.FindDate(DayOfWeek.Tuesday)
                        },
                        Wednesday =
                        {
                            TotalQuota = coachDaysQuota.WednesdayQuota, RemainQuota = wednesdayRemainQuota,
                            IsAbleToReserve = wednesdayRemainQuota > 0,
                            Date = DateUtils.FindDate(DayOfWeek.Wednesday)
                        },
                        Thursday =
                        {
                            TotalQuota = coachDaysQuota.ThursdayQuota, RemainQuota = thursdayRemainQuota,
                            IsAbleToReserve = thursdayRemainQuota > 0,
                            Date = DateUtils.FindDate(DayOfWeek.Thursday)
                        },
                        Friday =
                        {
                            TotalQuota = coachDaysQuota.FridayQuota, RemainQuota = fridayRemainQuota,
                            IsAbleToReserve = fridayRemainQuota > 0,
                            Date = DateUtils.FindDate(DayOfWeek.Friday),
                        },
                        Saturday =
                        {
                            TotalQuota = coachDaysQuota.SaturdayQuota, RemainQuota = saturdayRemainQuota,
                            IsAbleToReserve = saturdayRemainQuota > 0,
                            Date = DateUtils.FindDate(DayOfWeek.Saturday),
                        },
                        Sunday =
                        {
                            TotalQuota = coachDaysQuota.SundayQuota, RemainQuota = sundayRemainQuota,
                            IsAbleToReserve = sundayRemainQuota > 0,
                            Date = DateUtils.FindDate(DayOfWeek.Sunday),
                        }
                    };
                    
                    daysAvailability.HasAvailableDay = daysAvailability.Monday.IsAbleToReserve ||
                                                       daysAvailability.Tuesday.IsAbleToReserve ||
                                                       daysAvailability.Wednesday.IsAbleToReserve ||
                                                       daysAvailability.Thursday.IsAbleToReserve ||
                                                       daysAvailability.Friday.IsAbleToReserve ||
                                                       daysAvailability.Saturday.IsAbleToReserve ||
                                                       daysAvailability.Sunday.IsAbleToReserve;

                    return TypedResults.Ok(daysAvailability);

                }).ProducesProblem(StatusCodes.Status400BadRequest).RequireAuthorization("Student").WithOpenApi()
            .WithTags(TagConstants.StudentsCoachListAndReserve);
    }
    
    public sealed class CoachDaysQuota
    {
        public byte MondayQuota { get; set; }
        public byte TuesdayQuota { get; set; }
        public byte WednesdayQuota { get; set; }
        public byte ThursdayQuota { get; set; }
        public byte FridayQuota { get; set; }
        public byte SaturdayQuota { get; set; }
        public byte SundayQuota { get; set; }
    }
    
    public sealed class DateQuota
    {
        public int TotalQuota { get; set; }
        
        public int RemainQuota { get; set; }
        
        public bool IsAbleToReserve { get; set; }
        
        public DateTime Date { get; set; }
    }

    public sealed class DaysAvailability
    {
        public bool HasAvailableDay { get; set; }
        public DateQuota Sunday { get; } = new DateQuota();
        public DateQuota Monday { get; } = new DateQuota();
        public DateQuota Tuesday { get; } = new DateQuota();
        public DateQuota Wednesday { get; } = new DateQuota();
        public DateQuota Thursday { get; } = new DateQuota();
        public DateQuota Friday { get; } = new DateQuota();
        public DateQuota Saturday { get; } = new DateQuota();
    }
}