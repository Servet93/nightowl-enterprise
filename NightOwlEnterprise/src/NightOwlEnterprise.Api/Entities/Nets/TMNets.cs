namespace NightOwlEnterprise.Api.Entities.Nets;

public class TMNets
{
    public Guid UserId { get; set; }
    
    public ApplicationUser User { get; set; } // İlişkiyi burada tanımlıyoruz
    
    //Matematik: (Max 30, Min 0)
    public byte? Mathematics { get; set; }
    //Geometri: (Max 10, Min 0)
    public byte? Geometry { get; set; }
    //Edebiyat: (Max 24, Min 0)
    public byte? Literature { get; set; }
    //Tarih: (Max 10, Min 0)
    public byte? History { get; set; }
    //Coğrafya: (Max 6, Min 0)
    public byte? Geography { get; set; }
}