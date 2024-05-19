namespace NightOwlEnterprise.Api.Entities.PrivateTutoring;

public class PrivateTutoringDil
{
    public Guid CoachId { get; set; }
    public ApplicationUser Coach { get; set; } // İlişkiyi burada tanımlıyoruz
    
    //YDT: (Max 80, Min 0)
    public bool YTD { get; set; }
}