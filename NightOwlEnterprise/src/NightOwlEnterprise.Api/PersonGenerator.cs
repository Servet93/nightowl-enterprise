using System.Reflection.Metadata;

namespace NightOwlEnterprise.Api;

public class Person
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool Gender { get; set; } // True for male, False for female
    public string Email { get; set; }
    public string Phone { get; set; }
    public string City { get; set; }
    public string Address { get; set; }
    
    public string HighSchoolName { get; set; } // Lise adı
    
    public float HighSchoolScore { get; set; } // Lise başarı puanı
}

public class PersonGenerator
{
    private static readonly List<string> MaleFirstNames = new List<string>
    {
        "Ahmet", "Mehmet", "Mustafa", "Ali", "Hüseyin", "Hasan",
        "İbrahim", "Ömer", "Osman", "İsmail", "Mahmut", "Yusuf",
        "Adem", "Kemal", "Turgut", "Kadir", "Hakan", "Erdem",
        "Ertuğrul", "Süleyman", "Yunus", "Kerim", "Murat", "Selim",
        "Berkay", "Kaan", "Kerem", "Emre", "Cem", "Kenan", "Polat",
        "Serkan", "Samet", "Raşit", "Yiğit", "Fatih", "Mesut", "Can",
        "Osman", "Sait", "Tolga", "Arda", "Murat", "Alper", "Yusuf"
    };

    private static readonly List<string> FemaleFirstNames = new List<string>
    {
        "Ayşe", "Fatma", "Emine", "Hatice", "Zeynep", "Hacer",
        "Melek", "Hülya", "Gül", "Şükran", "Merve", "Melike",
        "Gizem", "Esra", "Sibel", "Aynur", "Zeliha", "Gülay",
        "Elif", "Nesrin", "Yasemin", "Nur", "Derya", "Şeyma",
        "Selma", "Duygu", "Büşra", "Nazlı", "Sema", "Dilek",
        "Melike", "Zehra", "Sema", "Arzu", "Selin", "Fitnat",
        "Özge", "Aytül", "Seda"
    };

    private static readonly List<string> LastNames = new List<string>
    {
        "Yılmaz", "Demir", "Şahin", "Yıldız", "Kaya", "Çelik",
        "Öztürk", "Kurt", "Doğan", "Kılıç", "Aydın", "Polat",
        "Yıldırım", "Akın", "Özdemir", "Tekin", "Karakuş",
        "Çetin", "Arslan", "Aslan", "Sır", "Kır", "Kel", "Tunç",
        "Bakır", 
    };

    private static readonly List<string> Cities = new List<string>
    {
        "İstanbul", "Ankara", "İzmir", "Bursa", "Antalya",
        "Konya", "Adana", "Gaziantep", "Mersin", "Kayseri",
        "Eskişehir", "Samsun", "Trabzon", "Denizli", "Malatya"
    };
    
    private static readonly List<string> HighSchoolNames = new List<string>
    {
        "Atatürk Lisesi", "Mehmet Akif Lisesi", "Şehitler Lisesi",
        "İstanbul Lisesi", "Hacettepe Lisesi", "Gazi Lisesi",
        "Mimar Sinan Lisesi", "Anadolu Lisesi", "Fen Lisesi",
        "Süleyman Demirel Lisesi", "Bursa Lisesi", "Gaziantep Lisesi",
        "Ankara Lisesi", "Çorum Lisesi", "Karşıyaka Lisesi"
    };

    private static readonly Random random = new Random();

    public static List<Person> GeneratePeople()
    {
        List<Person> people = new List<Person>();

        string lastName = string.Empty;
        string city = string.Empty;
        string address = string.Empty;
        string email = string.Empty;
        string phone = string.Empty;
        string highSchoolName = string.Empty;
        float highSchoolScore = 0f; // 0 ile 100 arasında rastgele başarı puanı
        int i = 1;        
        foreach (var maleFirstName in MaleFirstNames)
        {
            lastName = LastNames[random.Next(LastNames.Count)];
            city = Cities[random.Next(Cities.Count)];
            address = $"Street {random.Next(1, 100)}";
            email = $"{maleFirstName.ReplaceTurkishCharacters().ToLower()}.{lastName.ReplaceTurkishCharacters().ToLower()}@example.com";
            phone = $"05{random.Next(100, 1000)}{random.Next(100, 10000)}";
            highSchoolName = HighSchoolNames[random.Next(HighSchoolNames.Count)];
            highSchoolScore = random.NextSingle() * 100; // 0 ile 100 arasında rastgele başarı puanı
            
            // Yeni bir Person nesnesi oluştur ve listesine ekleyin
            Person person = new Person
            {
                Id = new Guid($"00000000-0000-0000-0000-{i.ToString("000000000000")}"),
                FirstName = maleFirstName,
                LastName = lastName,
                Gender = false,
                Email = email,
                Phone = phone,
                City = city,
                Address = address,
                HighSchoolName = highSchoolName,
                HighSchoolScore = highSchoolScore,
            };
            
            people.Add(person);

            i++;
        }

        foreach (var femaleFirstName in FemaleFirstNames)
        {
            lastName = LastNames[random.Next(LastNames.Count)];
            city = Cities[random.Next(Cities.Count)];
            address = $"Street {random.Next(1, 100)}";
            email = $"{femaleFirstName.ReplaceTurkishCharacters().ToLower()}.{lastName.ReplaceTurkishCharacters().ToLower()}@example.com";
            phone = $"05{random.Next(100, 1000)}{random.Next(100, 10000)}";
            
            // Yeni bir Person nesnesi oluştur ve listesine ekleyin
            Person person = new Person
            {
                Id = new Guid($"00000000-0000-0000-0000-{i.ToString("000000000000")}"),
                FirstName = femaleFirstName,
                LastName = lastName,
                Gender = false,
                Email = email,
                Phone = phone,
                City = city,
                Address = address,
                HighSchoolName = highSchoolName,
                HighSchoolScore = highSchoolScore,
            };

            people.Add(person);
            
            i++;
        }

        return people;
    }
}