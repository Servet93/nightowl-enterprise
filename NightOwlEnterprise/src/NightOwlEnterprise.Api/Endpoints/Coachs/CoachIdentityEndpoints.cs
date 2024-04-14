using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace NightOwlEnterprise.Api.Endpoints.Coachs;

public static class CoachIdentityEndpoints
{
    public static IEndpointConventionBuilder MapCoachsIdentityApi<TUser>(this IEndpointRouteBuilder endpoints)
        where TUser : class, new()
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        
        var emailSender = endpoints.ServiceProvider.GetRequiredService<IEmailSender<TUser>>();
        var linkGenerator = endpoints.ServiceProvider.GetRequiredService<LinkGenerator>();
        var stripeCredential = endpoints.ServiceProvider.GetRequiredService<IOptions<StripeCredential>>()?.Value;
        var stripeCredentialSigningSecret = stripeCredential?.SigningSecret;
        
        var jwtHelper = endpoints.ServiceProvider.GetRequiredService<JwtHelper>();
        //var jwtConfig = endpoints.ServiceProvider.GetRequiredService<IOptions<JwtConfig>>()?.Value;
        
        var routeGroup = endpoints.MapGroup("coachs").WithOpenApi().WithTags("Koç");

        routeGroup.MapRegister<ApplicationUser>((IEmailSender<ApplicationUser>)emailSender,
            linkGenerator);
        routeGroup.MapList();
        routeGroup.MapCalendar();
        // routeGroup.MapPayment<ApplicationUser>(stripeCredentialSigningSecret);
        // routeGroup.MapLogin<ApplicationUser>(jwtHelper);
        // routeGroup.MapRefresh<ApplicationUser>(jwtHelper);
        //routeGroup.MapConfirmEmail<ApplicationUser>();
        //routeGroup.MapResendConfirmationEmail<TUser>(emailSender, linkGenerator);
        // routeGroup.MapForgotPassword<ApplicationUser>((IEmailSender<ApplicationUser>)emailSender);
        // routeGroup.MapResetPassword<ApplicationUser>();
        // routeGroup.MapManageInfo<TUser>();
        // routeGroup.MapOnboard();

        return new IdentityEndpointsConventionBuilder(routeGroup);
    }


    
}
