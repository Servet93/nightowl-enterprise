using Microsoft.AspNetCore.Identity;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public class TurkishIdentityErrorDescriber : IdentityErrorDescriber
{
    public IdentityError InvalidName(string name)
    {
        return new IdentityError
        {
            Code = nameof(InvalidName),
            Description = $"İsim '{name}' geçersiz"
        };
    }
    
    public IdentityError InvalidSurname(string surname)
    {
        return new IdentityError
        {
            Code = nameof(InvalidSurname),
            Description = $"Soyisim '{surname}' geçersiz"
        };
    }
    
    public IdentityError RequiredAddress()
    {
        return new IdentityError
        {
            Code = nameof(RequiredAddress),
            Description = $"Adres bilgisi gerekli"
        };
    }
    
    public IdentityError InvalidCity(string city)
    {
        return new IdentityError
        {
            Code = nameof(InvalidCity),
            Description = $"Şehir '{city}' geçersiz"
        };
    }
    
    public override IdentityError DuplicateEmail(string email)
    {
        return new IdentityError
        {
            Code = nameof(DuplicateEmail),
            Description = $"Email '{email}' kayıtlı"
        };
    }

    public override IdentityError DuplicateUserName(string userName)
    {
        return new IdentityError
        {
            Code = nameof(DuplicateUserName),
            Description = $"Kullanıcı adı '{userName}' kayıtlı"
        };
    }

    public override IdentityError InvalidEmail(string? email)
    {
        return new IdentityError
        {
            Code = nameof(InvalidEmail),
            Description = $"Email '{email}' geçersiz"
        };
    }

    public override IdentityError DuplicateRoleName(string role)
    {
        return new IdentityError
        {
            Code = nameof(DuplicateRoleName),
            Description = $"Rol '{role}' kayıtlı"
        };
    }

    public override IdentityError InvalidRoleName(string? role)
    {
        return new IdentityError
        {
            Code = nameof(InvalidRoleName),
            Description = $"Rol '{role}' geçersiz"
        };
    }

    public override IdentityError InvalidToken()
    {
        return new IdentityError
        {
            Code = nameof(InvalidToken),
            Description = $"Token geçersiz"
        };
    }

    public override IdentityError InvalidUserName(string? userName)
    {
        return new IdentityError
        {
            Code = nameof(InvalidUserName),
            Description = $"Kullanıcı adı '{userName}' geçersiz"
        };
    }

    public override IdentityError LoginAlreadyAssociated()
    {
        return new IdentityError
        {
            Code = nameof(LoginAlreadyAssociated),
            Description = $"Kullanıcının aktif bir oturumu var"
        };
    }

    public override IdentityError PasswordMismatch()
    {
        return new IdentityError
        {
            Code = nameof(PasswordMismatch),
            Description = $"Şifre geçersiz"
        };
    }

    public override IdentityError PasswordRequiresDigit()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresDigit),
            Description = $"Şifre rakam içermelidir (0 - 9)"
        };
    }

    public override IdentityError PasswordRequiresLower()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresLower),
            Description = $"Şifre küçük harf içermelidir (a -z)"
        };
    }

    public override IdentityError PasswordRequiresNonAlphanumeric()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresNonAlphanumeric),
            Description = $"Şifre harf-rakam olmayan karakterler içermelidir (! + . - ? ...)"
        };
    }

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresUniqueChars),
            Description = $"Şifre en az '{uniqueChars} benzersiz karakter içermelidir"
        };
    }

    public override IdentityError PasswordRequiresUpper()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresUpper),
            Description = $"Şifre büyük harf içermelidir (A - Z)"
        };
    }

    public override IdentityError PasswordTooShort(int length)
    {
        return new IdentityError
        {
            Code = nameof(PasswordTooShort),
            Description = $"Şifre en az '{length}' karakter uzunluğunda olmalı"
        };
    }

    public override IdentityError UserAlreadyHasPassword()
    {
        return new IdentityError
        {
            Code = nameof(UserAlreadyHasPassword),
            Description = $"Kullanıcının bir şifresi zaten var"
        };
    }

    public override IdentityError UserAlreadyInRole(string role)
    {
        return new IdentityError
        {
            Code = nameof(UserAlreadyInRole),
            Description = $"Kullanıcı '{role}' rolüne sahip"
        };
    }

    public override IdentityError UserNotInRole(string role)
    {
        return new IdentityError
        {
            Code = nameof(UserNotInRole),
            Description = $"Kullanıcının '{role}' rolü yok"
        };
    }

    public override IdentityError UserLockoutNotEnabled()
    {
        return new IdentityError
        {
            Code = nameof(UserLockoutNotEnabled),
            Description = $"Kullanıcı kilidi aktif değil"
        };
    }

    public override IdentityError RecoveryCodeRedemptionFailed()
    {
        return new IdentityError
        {
            Code = nameof(RecoveryCodeRedemptionFailed),
            Description = $"Kurtarma kodu kullanılamadı"
        };
    }

    public override IdentityError ConcurrencyFailure()
    {
        return new IdentityError
        {
            Code = nameof(ConcurrencyFailure),
            Description = $"Eş-zamanlama hatası"
        };
    }

    public override IdentityError DefaultError()
    {
        return new IdentityError
        {
            Code = nameof(DefaultError),
            Description = $"Kullanıcı yönetim sistemi hatası"
        };
    }
}