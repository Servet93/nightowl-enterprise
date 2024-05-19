namespace NightOwlEnterprise.Api.Entities.Nets;

public class MFNets
{
    public Guid UserId { get; set; }
    
    public ApplicationUser User { get; set; } // İlişkiyi burada tanımlıyoruz
    
    //Matematik: (Max 30, Min 0)
    public byte? Mathematics { get; set; }
    //Geometri: (Max 10, Min 0)
    public byte? Geometry { get; set; }
    //Fizik: (Max 14, Min 0)
    public byte? Physics { get; set; }
    //Kimya: (Max 13, Min 0)
    public byte? Chemistry { get; set; }
    //Biology: (Max 13, Min 0)
    public byte? Biology { get; set; }
}