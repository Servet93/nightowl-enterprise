using Microsoft.AspNetCore.Identity;
using NightOwlEnterprise.Api.Entities;
using NightOwlEnterprise.Api.Entities.Enums;
using NightOwlEnterprise.Api.Entities.Nets;
using NightOwlEnterprise.Api.Entities.PrivateTutoring;

namespace NightOwlEnterprise.Api;

public class SeedDataService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public SeedDataService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<University> universities = new List<University>();
        int universitiesCount = 0;
        
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Coachları veritabanına ekleyecek kodu buraya yazın
            var hasAnyCoach = dbContext.Users.Any(x => x.UserType == UserType.Coach);

            if (hasAnyCoach)
            {
                return;
            }
            
            universities = dbContext.Universities.ToList();
            universitiesCount = universities.Count;
        }

        var isCompleted = false;

        var persons = PersonGenerator.GeneratePeople();

        var personsChunks = persons.Chunk(5).ToList();

        var personsChunkCount = personsChunks.Count();

        var personsChunkCounter = 0;

        while (!stoppingToken.IsCancellationRequested && personsChunkCounter < personsChunkCount)
        {
            // Arka planda yapılacak işleri burada gerçekleştirin

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                // Coachları db'ye eklemek için Seed işlemini gerçekleştirin
                await SeedCoachsAsync(personsChunks[personsChunkCounter].ToList(), universities, universitiesCount, dbContext, userManager)
                    .ConfigureAwait(false);
                personsChunkCounter++;
            }

            // İşlemi belirli bir süre bekletin (örneğin 1 gün)
            await Task.Delay(TimeSpan.FromMilliseconds(2000), stoppingToken);
        }
    }

    private async Task SeedCoachsAsync(List<Person> persons, List<University> universities, int universitiesCount, ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        var rnd = new Random();

        var index = 0;
        
        foreach (var person in persons)
        {
            index++;

            var x = (byte)rnd.Next(5);

            DepartmentType departmentType = DepartmentType.Sozel;

            var remain = index % 4;

            if (remain == 1)
            {
                departmentType = DepartmentType.TM;
            }
            else if (remain == 2)
            {
                departmentType = DepartmentType.MF;
            }
            else if (remain == 3)
            {
                departmentType = DepartmentType.Sozel;
            }
            else if (remain == 4)
            {
                departmentType = DepartmentType.Dil;
            }

            var mondayQuota = x == 0 ? (byte)1 : (byte)rnd.Next(5);
            var tuesdayQuota = x == 1 ? (byte)1 : (byte)rnd.Next(5);
            var wednesdayQuota = x == 2 ? (byte)1 : (byte)rnd.Next(5);
            var thursayQuota = x == 3 ? (byte)1 : (byte)rnd.Next(5);
            var fridayQuota = x == 4 ? (byte)1 : (byte)rnd.Next(5);

            var studentQuota = (byte)(mondayQuota + tuesdayQuota + wednesdayQuota + thursayQuota + fridayQuota);

            var applicationUser = new ApplicationUser()
            {
                Id = person.Id,
                Name = person.FirstName.ReplaceTurkishCharacters() + " " + person.LastName.ReplaceTurkishCharacters(),
                UserName = person.FirstName.ReplaceTurkishCharacters() + person.LastName.ReplaceTurkishCharacters(),
                Email = person.Email,
                PhoneNumber = person.Phone,
                City = person.City,
                Address = person.Address,
                CoachDetail = new CoachDetail()
                {
                    Name = person.FirstName,
                    Surname = person.LastName,
                    Email = person.Email,
                    Male = person.Gender,
                    BirthDate = DateTime.Now,
                    DepartmentType = departmentType,
                    DepartmentName = $"Department Name -> {rnd.Next(universitiesCount)}",
                    Mobile = person.Phone,
                    UniversityId = universities[rnd.Next(universitiesCount)].Id,
                    FirstTytNet = (byte)rnd.Next(90, 120),
                    LastTytNet = (byte)rnd.Next(90, 120),
                    FirstAytNet = (byte)rnd.Next(60, 80),
                    LastAytNet = (byte)rnd.Next(60, 80),
                    School = rnd.Next(100) % 2 == 0,
                    UsedYoutube = rnd.Next(100) % 2 == 0,
                    GoneCramSchool = rnd.Next(100) % 2 == 0,
                    Rank = (uint)rnd.Next(5000),
                    IsGraduated = rnd.Next(6) != 1 ? true : false,
                    HighSchool = person.HighSchoolName,
                    HighSchoolGPA = person.HighSchoolScore,
                    StudentQuota = studentQuota,
                    MondayQuota = mondayQuota,
                    TuesdayQuota = tuesdayQuota,
                    WednesdayQuota = wednesdayQuota,
                    ThursdayQuota = thursayQuota,
                    FridayQuota = fridayQuota,
                    PrivateTutoring = true,
                    Status = CoachStatus.Active,
                },
                TytNets = new TYTNets()
                {
                    Biology = (byte)rnd.Next(6),
                    Chemistry = (byte)rnd.Next(7),
                    Geometry = (byte)rnd.Next(10),
                    Physics = (byte)rnd.Next(7),
                    Mathematics = (byte)rnd.Next(30),
                    Geography = (byte)rnd.Next(5),
                    History = (byte)rnd.Next(5),
                    Philosophy = (byte)rnd.Next(5),
                    Religion = (byte)rnd.Next(5),
                    Turkish = (byte)rnd.Next(40),
                },
                UserType = UserType.Coach,
            };

            for (int j = 2016; j < 2024; j++)
            {
                var isEntered = rnd.Next(10) % 2 == 0;
                var rank = isEntered ? rnd.Next(5000) : 0;
                applicationUser.CoachYksRankings.Add(new CoachYksRanking()
                    { Year = j.ToString(), Enter = isEntered, Rank = (uint)rank });
            }

            if (departmentType == DepartmentType.MF)
            {
                applicationUser.MfNets = new MFNets()
                {
                    Biology = (byte)rnd.Next(13),
                    Chemistry = (byte)rnd.Next(13),
                    Geometry = (byte)rnd.Next(10),
                    Physics = (byte)rnd.Next(14),
                    Mathematics = (byte)rnd.Next(30),
                };

                applicationUser.PrivateTutoringMF = new PrivateTutoringMF()
                {
                    Biology = rnd.Next(2) % 2 == 0,
                    Chemistry = rnd.Next(2) % 2 == 0,
                    Geometry = rnd.Next(2) % 2 == 0,
                    Mathematics = rnd.Next(2) % 2 == 0,
                    Physics = rnd.Next(2) % 2 == 0,
                };

            }
            else if (departmentType == DepartmentType.TM)
            {
                applicationUser.TmNets = new TMNets()
                {
                    Geography = (byte)rnd.Next(6),
                    History = (byte)rnd.Next(10),
                    Geometry = (byte)rnd.Next(10),
                    Literature = (byte)rnd.Next(24),
                    Mathematics = (byte)rnd.Next(30),
                };

                applicationUser.PrivateTutoringTM = new PrivateTutoringTM()
                {
                    Geography = rnd.Next(2) % 2 == 0,
                    Geometry = rnd.Next(2) % 2 == 0,
                    History = rnd.Next(2) % 2 == 0,
                    Mathematics = rnd.Next(2) % 2 == 0,
                    Literature = rnd.Next(2) % 2 == 0,
                };
            }
            else if (departmentType == DepartmentType.Sozel)
            {
                applicationUser.SozelNets = new SozelNets()
                {
                    Geography1 = (byte)rnd.Next(24),
                    Geography2 = (byte)rnd.Next(11),
                    History1 = (byte)rnd.Next(10),
                    History2 = (byte)rnd.Next(11),
                    Literature1 = (byte)rnd.Next(6),
                    Philosophy = (byte)rnd.Next(12),
                    Religion = (byte)rnd.Next(6),
                };

                applicationUser.PrivateTutoringSozel = new PrivateTutoringSozel()
                {
                    Geography1 = rnd.Next(2) % 2 == 0,
                    Geography2 = rnd.Next(2) % 2 == 0,
                    History1 = rnd.Next(2) % 2 == 0,
                    History2 = rnd.Next(2) % 2 == 0,
                    Literature1 = rnd.Next(2) % 2 == 0,
                    Philosophy = rnd.Next(2) % 2 == 0,
                    Religion = rnd.Next(2) % 2 == 0,
                };

            }
            else if (departmentType == DepartmentType.Dil)
            {
                applicationUser.DilNets = new DilNets()
                {
                    YDT = (byte)rnd.Next(80),
                };

                applicationUser.PrivateTutoringDil = new PrivateTutoringDil()
                {
                    YTD = rnd.Next(2) % 2 == 0,
                };
            }

            try
            {
                await userManager.CreateAsync(applicationUser, "Aa123456").ConfigureAwait(false);
                // await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}