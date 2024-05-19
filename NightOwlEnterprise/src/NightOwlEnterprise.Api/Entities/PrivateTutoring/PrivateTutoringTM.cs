namespace NightOwlEnterprise.Api.Entities.PrivateTutoring;

public class PrivateTutoringTM
{
    public Guid CoachId { get; set; }
    public ApplicationUser Coach { get; set; } // İlişkiyi burada tanımlıyoruz
    
    //Matematik: (Max 30, Min 0)
    public bool Mathematics { get; set; }
    //Geometri: (Max 10, Min 0)
    public bool Geometry { get; set; }
    //Edebiyat: (Max 24, Min 0)
    public bool Literature { get; set; }
    //Tarih: (Max 10, Min 0)
    public bool History { get; set; }
    //Coğrafya: (Max 6, Min 0)
    public bool Geography { get; set; }
}