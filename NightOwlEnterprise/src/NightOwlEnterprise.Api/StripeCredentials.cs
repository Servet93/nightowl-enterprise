namespace NightOwlEnterprise.Api;

public class StripeCredential
{
    public const string Stripe = "Stripe";

    //For Backend
    public string? SecretKey { get; init; } = null;

    //For Client(Browser,Mobile)
    public string? PublishableKey { get; init; } = null;
    
    //For Webhook
    public string? SigningSecret { get; init; } = null;
}