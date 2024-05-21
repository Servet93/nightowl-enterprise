namespace NightOwlEnterprise.Api.Entities;

public class CoachYksRanking
{
    public Guid Id { get; set; }
    
    public Guid CoachId { get; set; }
    
    public ApplicationUser Coach { get; set; } // İlişkiyi burada tanımlıyoruz

    public string Year { get; set; }
    
    public bool Enter { get; set; }
    
    public uint? Rank { get; set; }
}