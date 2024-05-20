namespace NightOwlEnterprise.Api.Entities;

public class ResourcesAYT
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } // İlişkiyi burada tanımlıyoruz
    
    public string? Mathematics { get; set; }
    public string? Geometry { get; set; }
    public string? Physics { get; set; }
    public string? Chemistry { get; set; }
    public string? Biology { get; set; }
    public string? Literature { get; set; }
    public string? History { get; set; }
    public string? Geography { get; set; }
    public string? Philosophy { get; set; }
    public string? Religion { get; set; }
    
    public string? Turkish { get; set; }
}