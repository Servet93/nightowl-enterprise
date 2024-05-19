namespace NightOwlEnterprise.Api.Entities.PrivateTutoring;

public class PrivateTutoringTYT
{
    public Guid CoachId { get; set; }
    public ApplicationUser Coach { get; set; } // İlişkiyi burada tanımlıyoruz
    
    public bool Turkish { get; set; }
    public bool Mathematics { get; set; }
    public bool Geometry { get; set; }
    public bool History { get; set; }
    public bool Geography { get; set; }
    public bool Philosophy { get; set; }
    public bool Religion { get; set; }
    public bool Physics { get; set; }
    public bool Chemistry { get; set; }
    public bool Biology { get; set; }
}