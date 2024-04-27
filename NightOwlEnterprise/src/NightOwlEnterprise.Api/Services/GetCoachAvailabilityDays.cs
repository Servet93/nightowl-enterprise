using Microsoft.EntityFrameworkCore;

namespace NightOwlEnterprise.Api.Services;

public class GetCoachAvailabilityDays
{
    public async Task<DaysAvailability> GetAsync(ApplicationDbContext dbContext, Guid coachId)
    {
        var quoato = await dbContext.CoachDetail.Where(x => x.CoachId == coachId).Select(x => new
        {
            MondayQuota = x.MondayQuota,
            TuesdayQuota = x.TuesdayQuota,
            WednesdayQuota = x.WednesdayQuota,
            ThursdayQuota = x.ThursdayQuota,
            FridayQuota = x.FridayQuota,
            SaturdayQuota = x.SaturdayQuota,
            SundayQuota = x.SundayQuota,
        }).FirstOrDefaultAsync();

        var daysMapping = new Dictionary<string, DateQuota>();

        var now = DateTime.Now;

        // Bugünkü tarihi al
        var weekDays = now.GetWeekDays();

        foreach (var day in weekDays)
        {
            var totalQuota = day.DayOfWeek switch
            {
                DayOfWeek.Sunday => quoato!.SundayQuota,
                DayOfWeek.Monday => quoato!.MondayQuota,
                DayOfWeek.Tuesday => quoato!.TuesdayQuota,
                DayOfWeek.Wednesday => quoato!.WednesdayQuota,
                DayOfWeek.Thursday => quoato!.ThursdayQuota,
                DayOfWeek.Friday => quoato!.FridayQuota,
                DayOfWeek.Saturday => quoato!.SaturdayQuota,
                _ => throw new ArgumentOutOfRangeException()
            };

            var isDatePast = day < now;

            daysMapping.Add(day.DayOfWeek.ToString(), new DateQuota()
            {
                Date = day,
                TotalQuota = totalQuota,
                RemainQuota = totalQuota,
                IsAbleToReserve = !isDatePast && totalQuota > 0,
                IsDatePast = isDatePast
            });
        }

        var dateToInvitesList = await dbContext.Invitations
            .Where(x => x.CoachId == coachId && weekDays.Contains(x.Date)).GroupBy(x => x.Date)
            .ToListAsync();

        foreach (var dateToInvites in dateToInvitesList)
        {
            if (daysMapping.ContainsKey(dateToInvites.Key.DayOfWeek.ToString()))
            {
                var total = daysMapping[dateToInvites.Key.DayOfWeek.ToString()].TotalQuota;
                var remain = (byte)(total - dateToInvites.Count());
                var isDatePast = daysMapping[dateToInvites.Key.DayOfWeek.ToString()].IsDatePast;
                daysMapping[dateToInvites.Key.DayOfWeek.ToString()].RemainQuota = remain;
                daysMapping[dateToInvites.Key.DayOfWeek.ToString()].IsAbleToReserve = !isDatePast && remain > 0;
            }
        }
        
        var daysAvailability = new DaysAvailability();
                
        FillDaysAvailability(daysAvailability.Sunday, daysMapping[DayOfWeek.Sunday.ToString()]);
        FillDaysAvailability(daysAvailability.Monday, daysMapping[DayOfWeek.Monday.ToString()]);
        FillDaysAvailability(daysAvailability.Tuesday, daysMapping[DayOfWeek.Tuesday.ToString()]);
        FillDaysAvailability(daysAvailability.Wednesday, daysMapping[DayOfWeek.Wednesday.ToString()]);
        FillDaysAvailability(daysAvailability.Thursday, daysMapping[DayOfWeek.Thursday.ToString()]);
        FillDaysAvailability(daysAvailability.Friday, daysMapping[DayOfWeek.Friday.ToString()]);
        FillDaysAvailability(daysAvailability.Saturday, daysMapping[DayOfWeek.Saturday.ToString()]);

        daysAvailability.HasAvailableDay = (daysAvailability.Sunday.IsAbleToReserve ||
                                            daysAvailability.Monday.IsAbleToReserve ||
                                            daysAvailability.Tuesday.IsAbleToReserve ||
                                            daysAvailability.Wednesday.IsAbleToReserve ||
                                            daysAvailability.Thursday.IsAbleToReserve ||
                                            daysAvailability.Friday.IsAbleToReserve ||
                                            daysAvailability.Saturday.IsAbleToReserve);

        return daysAvailability;
    }
    
    public DaysAvailability Get(ApplicationDbContext dbContext, Guid coachId)
    {
        var quoato = dbContext.CoachDetail.Where(x => x.CoachId == coachId).Select(x => new
        {
            MondayQuota = x.MondayQuota,
            TuesdayQuota = x.TuesdayQuota,
            WednesdayQuota = x.WednesdayQuota,
            ThursdayQuota = x.ThursdayQuota,
            FridayQuota = x.FridayQuota,
            SaturdayQuota = x.SaturdayQuota,
            SundayQuota = x.SundayQuota,
        }).FirstOrDefault();

        var daysMapping = new Dictionary<string, DateQuota>();

        var now = DateTime.Now;

        // Bugünkü tarihi al
        var weekDays = now.GetWeekDays();

        foreach (var day in weekDays)
        {
            var totalQuota = day.DayOfWeek switch
            {
                DayOfWeek.Sunday => quoato!.SundayQuota,
                DayOfWeek.Monday => quoato!.MondayQuota,
                DayOfWeek.Tuesday => quoato!.TuesdayQuota,
                DayOfWeek.Wednesday => quoato!.WednesdayQuota,
                DayOfWeek.Thursday => quoato!.ThursdayQuota,
                DayOfWeek.Friday => quoato!.FridayQuota,
                DayOfWeek.Saturday => quoato!.SaturdayQuota,
                _ => throw new ArgumentOutOfRangeException()
            };

            var isDatePast = day < now;

            daysMapping.Add(day.DayOfWeek.ToString(), new DateQuota()
            {
                Date = day,
                TotalQuota = totalQuota,
                RemainQuota = totalQuota,
                IsAbleToReserve = !isDatePast,
                IsDatePast = isDatePast
            });
        }

        var dateToInvitesList = dbContext.Invitations
            .Where(x => x.CoachId == coachId && weekDays.Contains(x.Date)).GroupBy(x => x.Date)
            .ToList();

        foreach (var dateToInvites in dateToInvitesList)
        {
            if (daysMapping.ContainsKey(dateToInvites.Key.DayOfWeek.ToString()))
            {
                var total = daysMapping[dateToInvites.Key.DayOfWeek.ToString()].TotalQuota;
                var remain = (byte)(total - dateToInvites.Count());
                var isDatePast = daysMapping[dateToInvites.Key.DayOfWeek.ToString()].IsDatePast;
                daysMapping[dateToInvites.Key.DayOfWeek.ToString()].RemainQuota = remain;
                daysMapping[dateToInvites.Key.DayOfWeek.ToString()].IsAbleToReserve = !isDatePast && remain > 0;
            }
        }
        
        var daysAvailability = new DaysAvailability();
                
        FillDaysAvailability(daysAvailability.Sunday, daysMapping[DayOfWeek.Sunday.ToString()]);
        FillDaysAvailability(daysAvailability.Monday, daysMapping[DayOfWeek.Monday.ToString()]);
        FillDaysAvailability(daysAvailability.Tuesday, daysMapping[DayOfWeek.Tuesday.ToString()]);
        FillDaysAvailability(daysAvailability.Wednesday, daysMapping[DayOfWeek.Wednesday.ToString()]);
        FillDaysAvailability(daysAvailability.Thursday, daysMapping[DayOfWeek.Thursday.ToString()]);
        FillDaysAvailability(daysAvailability.Friday, daysMapping[DayOfWeek.Friday.ToString()]);
        FillDaysAvailability(daysAvailability.Saturday, daysMapping[DayOfWeek.Saturday.ToString()]);

        daysAvailability.HasAvailableDay = (daysAvailability.Sunday.IsAbleToReserve ||
                                            daysAvailability.Monday.IsAbleToReserve ||
                                            daysAvailability.Tuesday.IsAbleToReserve ||
                                            daysAvailability.Wednesday.IsAbleToReserve ||
                                            daysAvailability.Thursday.IsAbleToReserve ||
                                            daysAvailability.Friday.IsAbleToReserve ||
                                            daysAvailability.Saturday.IsAbleToReserve);

        return daysAvailability;
    }
    
    void FillDaysAvailability(DateQuota destination, DateQuota source)
    {
        destination.Date = source.Date;
        destination.TotalQuota = source.TotalQuota;
        destination.RemainQuota = source.RemainQuota;
        destination.IsAbleToReserve = source.IsAbleToReserve;
        destination.IsDatePast = source.IsDatePast;
    }
}

public sealed class DateQuota
{
    public DateTime Date { get; set; }
        
    public byte TotalQuota { get; set; }
        
    public byte RemainQuota { get; set; }
        
    public bool IsAbleToReserve { get; set; }
        
    public bool IsDatePast { get; set; }
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