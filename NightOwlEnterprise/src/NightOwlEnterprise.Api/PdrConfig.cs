namespace NightOwlEnterprise.Api;

public class PdrConfig
{
    public const string PdrSection = "Pdr";
    
    public byte? MinQuota { get; set; }
    
    public byte? MaxQuota { get; set; }
}