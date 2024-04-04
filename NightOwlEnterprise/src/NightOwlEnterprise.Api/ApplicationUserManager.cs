using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace NightOwlEnterprise.Api;

public class ApplicationUserManager : UserManager<ApplicationUser>
{
    public ApplicationUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators, IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        
    }

    public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string newPassword)
    {
        var result = await UpdatePasswordHash(user, newPassword, validatePassword: true).ConfigureAwait(false);
        
        if (!result.Succeeded)
        {
            return result;
        }
        
        return await UpdateUserAsync(user).ConfigureAwait(false);
    }
}