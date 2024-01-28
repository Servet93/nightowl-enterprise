using Microsoft.AspNetCore.Identity.UI.Services;

namespace NightOwlEnterprise.Api.Utils.Email;

public class LocalEmailSender : IEmailSender
{
    private ILogger<LocalEmailSender> _logger;
    public LocalEmailSender(ILogger<LocalEmailSender> logger)
    {
        _logger = logger;
    }
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation($"{email} -> {subject} -> {htmlMessage}");
        return Task.CompletedTask;
    }
    
}