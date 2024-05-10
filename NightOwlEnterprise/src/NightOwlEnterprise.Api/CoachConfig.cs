namespace NightOwlEnterprise.Api;

public class CoachConfig
{
    public const string CoachSection = "Coach";
    
    public byte? MinQuota { get; set; }
    
    public byte? MaxQuota { get; set; }
}