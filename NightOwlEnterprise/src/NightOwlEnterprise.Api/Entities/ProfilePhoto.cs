namespace NightOwlEnterprise.Api.Entities;

public class ProfilePhoto
{
    public Guid UserId { get; set; }
    
    public ApplicationUser User { get; set; } // İlişkiyi burada tanımlıyoruz

    public string? Photo { get; set; }
}