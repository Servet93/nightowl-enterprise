using NightOwlEnterprise.Api.Entities;
using NightOwlEnterprise.Api.Entities.Enums;
using NightOwlEnterprise.Api.Utils;

namespace NightOwlEnterprise.Api;

public class ProgramAndInvitations
{
    public static void Create(ApplicationDbContext dbContext, Guid studentId)
    {
        var turkishCulture = new System.Globalization.CultureInfo("tr-TR");

        var coachAndVideoDay = dbContext.CoachStudentTrainingSchedules.OrderByDescending(x => x.CreatedAt)
            .Where(x => x.StudentId == studentId)
            .Select(x => new
            {
                CoachId = x.CoachId,
                VideoDay = x.VideoDay,
            }).FirstOrDefault();

        var programStartingDate = coachAndVideoDay!.VideoDay!.Value;
        var coachId = coachAndVideoDay.CoachId;
        
        var date = DateUtils.FindDate(programStartingDate);

        dbContext.CoachStudentTrainingSchedules.Add(new CoachStudentTrainingSchedule()
        {
            CoachId = coachId,
            StudentId = studentId,
            VideoDay = programStartingDate,
            VoiceDay = date.AddDays(3).DayOfWeek,
            CreatedAt = DateTime.UtcNow.ConvertUtcToTimeZone()
        });

        var studentProgram = new StudentProgram()
        {
            CoachId = coachId,
            StudentId = studentId,
            StartDate = date,
            StartDateText = date.ToString("dd MMMM yyyy, dddd", turkishCulture),
            CreatedAt = DateTime.UtcNow.ConvertUtcToTimeZone()
        };

        for (int i = 1; i <= 4; i++)
        {
            var weeklyStartDate = date;

            dbContext.Invitations.Add(new Invitation()
            {
                CoachId = coachId,
                StudentId = studentId,
                State = InvitationState.SpecifyHour,
                Type = InvitationType.VideoCall,
                Date = date,
                Day = date.DayOfWeek,
            });

            dbContext.Invitations.Add(new Invitation()
            {
                CoachId = coachId,
                StudentId = studentId,
                State = InvitationState.SpecifyHour,
                Type = InvitationType.VoiceCall,
                Date = date.AddDays(3),
                Day = date.AddDays(3).DayOfWeek
            });

            date = date.AddDays(7);

            var weeklyEndDate = date.AddDays(-1);

            var weekly = new StudentProgramWeekly()
            {
                CoachId = coachId,
                StudentId = studentId,
                StartDate = weeklyStartDate,
                StartDateText = weeklyStartDate.ToString("dd MMMM yyyy, dddd", turkishCulture),
                EndDate = weeklyEndDate,
                EndDateText = date.ToString("dd MMMM yyyy, dddd", turkishCulture),
                CreatedAt = DateTime.UtcNow.ConvertUtcToTimeZone()
            };

            for (DateTime dailyStartDate = weeklyStartDate;
                 dailyStartDate <= weeklyEndDate;
                 dailyStartDate = dailyStartDate.AddDays(1))
            {
                var dayText = turkishCulture.DateTimeFormat.GetDayName(dailyStartDate.DayOfWeek);

                weekly.Dailies.Add(new StudentProgramDaily()
                {
                    StudentId = studentId,
                    CoachId = coachId,
                    Date = dailyStartDate,
                    DateText = dailyStartDate.ToString("dd MMMM yyyy, dddd", turkishCulture),
                    Day = dailyStartDate.DayOfWeek,
                    DayText = dayText,
                    CreatedAt = DateTime.UtcNow.ConvertUtcToTimeZone()
                });
            }

            studentProgram.Weeklies.Add(weekly);
        }

        studentProgram.EndDate = date;
        studentProgram.EndDateText = date.ToString("dd MMMM yyyy, dddd", turkishCulture);

        dbContext.StudentPrograms.Add(studentProgram);
    }
}