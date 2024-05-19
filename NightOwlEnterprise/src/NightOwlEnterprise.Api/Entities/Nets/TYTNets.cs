namespace NightOwlEnterprise.Api.Entities.Nets;

public class TYTNets
{
    public Guid UserId { get; set; }
    
    public ApplicationUser User { get; set; } // İlişkiyi burada tanımlıyoruz
    
    //Anlam Bilgisi: (Max 30, Min 0)
    public byte? Semantics { get; set; }
    //Dil Bilgisi: (Max 10, Min 0)
    public byte? Grammar { get; set; }
    //Matematik: (Max 30, Min 0)
    public byte? Mathematics { get; set; }
    //Geometri: (Max 10, Min 0)
    public byte? Geometry { get; set; }
    //Tarih: (Max 5, Min 0)
    public byte? History { get; set; }
    //Coğrafya: (Max 5, Min 0)
    public byte? Geography { get; set; }
    //Felsefe: (Max 5, Min 0)
    public byte? Philosophy { get; set; }
    //Din: (Max 5, Min 0)
    public byte? Religion { get; set; }
    //Fizik: (Max 7, Min 0)
    public byte? Physics { get; set; }
    //Kimya: (Max 7, Min 0)
    public byte? Chemistry { get; set; }
    //Biology: (Max 6, Min 0)
    public byte? Biology { get; set; }
}