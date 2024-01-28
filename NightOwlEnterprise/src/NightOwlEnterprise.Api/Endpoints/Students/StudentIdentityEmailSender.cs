using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public class StudentIdentityEmailSender : IEmailSender<StudentApplicationUser> 
{
    private IEmailSender _emailSender;

    public StudentIdentityEmailSender(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public Task SendConfirmationLinkAsync(StudentApplicationUser user, string email, string confirmationLink)
    {
        var mailTemplate = ConfirmationEmailTemplate(user.Name, user.Surname, confirmationLink);
        return _emailSender.SendEmailAsync(email, "Confirm your email", mailTemplate);
    }

    private static string ConfirmationEmailTemplate(string name, string surname, string confirmationLink)
    {
        string title = "Confirmation Email";

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
    <p>Merhaba {name} {surname},</p>
    <p>Hesabınız başarıyla oluşturuldu. Lütfen aşağıdaki bağlantıya tıklayarak hesabınızı onaylayın:</p>
    <p><a href=""{confirmationLink}"" style=""color: #3498db; text-decoration: none;"" target=""_blank"">Onay Bağlantısı</a></p>
    <p>Teşekkür ederiz!</p>
</body>
</html>";

        return htmlContent;
    }

    private static string PasswordResetCodeEmailTemplate(string name, string surname, string resetCode)
    {
        string htmlContent = $@"<!DOCTYPE html>
<html lang=""en"">
    <head>
    <meta charset=""UTF-8"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Password Reset Email</title>
    <style>
        body {{
        font-family: 'Arial', sans-serif;
        max-width: 600px;
        margin: 20px auto;
        background-color: #f4f4f4;
        padding: 20px;
        }}
        h2 {{
            color: #e74c3c;
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
        <h2>Password Reset Email</h2>
        <p>Merhaba {name} {surname},</p>
        <p>Şifrenizin sıfırlanmasını talep ettiniz. Lütfen aşağıdaki kodu kullanın:</p>
        <p><strong>{resetCode}</strong></p>
        <p>Bunu siz talep etmediyseniz bu e-postayı güvenle yok sayabilirsiniz.</p>
        <p>Teşekkür ederiz!</p>
        </body>
</html>";

        return htmlContent;
    }

    public Task SendPasswordResetLinkAsync(StudentApplicationUser user, string email, string resetLink)
    {
        var mailTemplate = PasswordResetCodeEmailTemplate(user.Name, user.Surname, string.Empty);
        return _emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");
    }

    public Task SendPasswordResetCodeAsync(StudentApplicationUser user, string email, string resetCode)
    {
        var mailTemplate = PasswordResetCodeEmailTemplate(user.Name, user.Surname, resetCode);
        return _emailSender.SendEmailAsync(email, "Reset your password", mailTemplate);
    }
}