namespace NightOwlEnterprise.Api.Entities.Nets;

public class DilNets
{
    public Guid UserId { get; set; }
    
    public ApplicationUser User { get; set; } // İlişkiyi burada tanımlıyoruz
    
    //YDT: (Max 80, Min 0) Yabacnı Dil Testi
    public byte? YDT { get; set; }
}