namespace NightOwlEnterprise.Api.Entities.PrivateTutoring;

public class PrivateTutoringMF
{
    public Guid CoachId { get; set; }
    public ApplicationUser Coach { get; set; } // İlişkiyi burada tanımlıyoruz
    
    public bool Mathematics { get; set; }
    public bool Geometry { get; set; }
    public bool Physics { get; set; }
    public bool Chemistry { get; set; }
    public bool Biology { get; set; }
}