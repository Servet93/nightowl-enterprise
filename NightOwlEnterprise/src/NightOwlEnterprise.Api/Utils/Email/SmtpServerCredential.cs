namespace NightOwlEnterprise.Api.Utils.Email;

public class SmtpServerCredential
{
    public const string SmtpServer = "SmtpServer";

    public string? Address { get; init; } = null;

    public int Port { get; init; } = 0;

    public string? Username { get; init; } = null;

    public string? Password { get; init; } = null;
    
    public bool EnableSsl { get; init; }

    public string? DisplayName { get; init; } = null;
}