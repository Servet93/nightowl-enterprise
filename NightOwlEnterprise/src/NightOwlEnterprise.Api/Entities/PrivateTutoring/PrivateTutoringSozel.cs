namespace NightOwlEnterprise.Api.Entities.PrivateTutoring;

public class PrivateTutoringSozel
{
    public Guid CoachId { get; set; }
    public ApplicationUser Coach { get; set; } // İlişkiyi burada tanımlıyoruz
    
    //Tarih-1: (Max 10, Min 0)
    public bool History1 { get; set; }
    //Coğrafya: (Max 24, Min 0)
    public bool Geography1 { get; set; }
    //Edebiyat-1: (Max 6, Min 0)
    public bool Literature1 { get; set; }
    //Tarih-2: (Max 11, Min 0)
    public bool History2 { get; set; }
    //Coğrafya-2: (Max 11, Min 0)
    public bool Geography2 { get; set; }
    //Felsefe: (Max 12, Min 0)
    public bool Philosophy { get; set; }
    //Din: (Max 6, Min 0)
    public bool Religion { get; set; }
}