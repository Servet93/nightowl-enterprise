namespace NightOwlEnterprise.Api;

public class AwsCloudWatchConfig
{
    public const string AwsCloudWatchConfigSection = "AwsCloudWatch";
    
    public bool Enabled { get; set; }
    
    public string? AccessKey { get; set; }
    
    public string? SecretKey { get; set; }
    
    public string? Region { get; set; }
    
    public string? LogGroup { get; set; }
}