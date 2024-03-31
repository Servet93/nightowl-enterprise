namespace NightOwlEnterprise.Api;

public class StripeCredential
{
    public const string StripeSection = "Stripe";

    //For Backend
    public string? SecretKey { get; set; }

    //For Client(Browser,Mobile)
    public string? PublishableKey { get; set; }
    
    //For Webhook
    public string? SigningSecret { get; set; }
}