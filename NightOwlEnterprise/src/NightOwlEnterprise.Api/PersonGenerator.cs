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

    public static List<Person> GeneratePeople(int count)
    {
        List<Person> people = new List<Person>();

        for (int i = 0; i < count; i++)
        {
            bool isMale = random.Next(0, 2) == 1; // Rastgele olarak cinsiyeti belirler

            // İlk isim listelerini cinsiyete göre seçin
            List<string> firstNameList = isMale ? MaleFirstNames : FemaleFirstNames;

            // Rastgele bir isim, soyisim, şehir ve adres oluştur
            string firstName = firstNameList[random.Next(firstNameList.Count)];
            string lastName = LastNames[random.Next(LastNames.Count)];
            string city = Cities[random.Next(Cities.Count)];
            string address = $"Street {random.Next(1, 100)}";

            // E-posta ve telefon numarası
            string email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com";
            string phone = $"05{random.Next(100, 1000)}{random.Next(100, 10000)}";
            
            // Lise adı ve başarı puanı
            string highSchoolName = HighSchoolNames[random.Next(HighSchoolNames.Count)];
            float highSchoolScore = random.NextSingle() * 100; // 0 ile 100 arasında rastgele başarı puanı

            // Yeni bir Person nesnesi oluştur ve listesine ekleyin
            Person person = new Person
            {
                Id = new Guid($"00000000-0000-0000-0000-{i.ToString("000000000000")}"),
                FirstName = firstName,
                LastName = lastName,
                Gender = isMale,
                Email = email,
                Phone = phone,
                City = city,
                Address = address,
                HighSchoolName = highSchoolName,
                HighSchoolScore = highSchoolScore,
            };

            people.Add(person);
        }

        return people;
    }
}