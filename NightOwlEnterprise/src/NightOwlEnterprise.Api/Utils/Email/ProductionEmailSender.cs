using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace NightOwlEnterprise.Api.Utils.Email;

public class ProductionEmailSender : IEmailSender
{
    private ILogger<ProductionEmailSender> _logger;
    private SmtpServerCredential _smtpServerCredential;
    
    public ProductionEmailSender(ILogger<ProductionEmailSender> logger, IOptions<SmtpServerCredential> smtpServerCredential)
    {
        _logger = logger;
        _smtpServerCredential = smtpServerCredential.Value;
    }
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        ArgumentException.ThrowIfNullOrEmpty(_smtpServerCredential.Username);
        
        // Gönderen e-posta adresi
        var fromAddress = new MailAddress(_smtpServerCredential.Username, _smtpServerCredential.DisplayName);

        // Alıcı e-posta adresi
        var toAddressObj = new MailAddress(email);

        // E-posta mesajı oluştur
        var mailMessage = new MailMessage(fromAddress, toAddressObj)
        {
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true // İsteğe bağlı: HTML içerik kullanılacaksa true yapın
        };

        // SMTP istemcisini oluştur
        var smtpClient = new SmtpClient(_smtpServerCredential.Address)
        {
            Port = _smtpServerCredential.Port,
            Credentials = new NetworkCredential(_smtpServerCredential.Username, _smtpServerCredential.Password),
            EnableSsl = _smtpServerCredential.EnableSsl // İsteğe bağlı: Güvenli bağlantı kullanılacaksa true yapın
        };

        try
        {
            // E-postayı gönder
            smtpClient.Send(mailMessage);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            //throw;
        }
        
        return Task.CompletedTask;
    }
    
}
