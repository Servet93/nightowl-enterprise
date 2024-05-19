namespace NightOwlEnterprise.Api.Entities.Nets;

public class SozelNets
{
    public Guid UserId { get; set; }
    
    public ApplicationUser User { get; set; } // İlişkiyi burada tanımlıyoruz
    
    //Tarih-1: (Max 10, Min 0)
    public byte? History1 { get; set; }
    //Coğrafya: (Max 24, Min 0)
    public byte? Geography1 { get; set; }
    //Edebiyat-1: (Max 6, Min 0)
    public byte? Literature1 { get; set; }
    //Tarih-2: (Max 11, Min 0)
    public byte? History2 { get; set; }
    //Coğrafya-2: (Max 11, Min 0)
    public byte? Geography2 { get; set; }
    //Felsefe: (Max 12, Min 0)
    public byte? Philosophy { get; set; }
    //Din: (Max 6, Min 0)
    public byte? Religion { get; set; }
}