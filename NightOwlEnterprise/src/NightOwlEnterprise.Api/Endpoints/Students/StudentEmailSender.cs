using Microsoft.AspNetCore.Identity.UI.Services;
using NightOwlEnterprise.Api.Entities;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public class StudentEmailSender
{
    private IEmailSender _emailSender;

    public StudentEmailSender(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }
    
    public Task SendSignInInfo(ApplicationUser user, string generatedPassword)
    {
        var mailTemplate = RegistrationCompletedEmailTemplate(user.Name, user.Email, generatedPassword);
        return _emailSender.SendEmailAsync(user.Email, "Kayıt işlemi tamamlandı", mailTemplate);
    }

    private string RegistrationCompletedEmailTemplate(string name, string email, string password)
    {
        string title = "Kayıt işlemi tamamlandı";

        string htmlContent = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{title}</title>
 <style>
        body {{
            font-family: Arial, sans-serif;
            max-width: 600px;
            margin: 20px auto;
            background-color: #f4f4f4;
            padding: 20px;
        }}
        h2 {{
            color: #3498db;
        }}
        p {{
            color: #555;
        }}
        img {{
            max-width: 150px;
            height: 150px;
            margin-bottom: 20px;
        }}
    </style>
</head>
<body>
    <img src=""https://cdn.pixabay.com/photo/2021/01/27/06/51/owl-5953875_1280.png"" alt=""Brand Logo"">
    <h2>{title}</h2>
    <p>Merhaba {name},</p>
    <p>Hesabınız başarıyla oluşturuldu. Aşağıdaki bilgileri kullanarak giriş yapabilirsiniz:</p>
    <p>Email: {email} ,</p>
    <p>Password: {password} ,</p>
    <p>Teşekkür ederiz!</p>
</body>
</html>";

        return htmlContent;
    }
    
}