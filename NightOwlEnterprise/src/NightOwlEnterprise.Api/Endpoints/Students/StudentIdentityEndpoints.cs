using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NightOwlEnterprise.Api.Endpoints.Students.Identity;
using NightOwlEnterprise.Api.Endpoints.Students.Me;
using NightOwlEnterprise.Api.Endpoints.Students.Me.Invitation;
using NightOwlEnterprise.Api.Entities;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public static class StudentIdentityEndpoints
{
    public static IEndpointConventionBuilder MapStudentsIdentityApi<TUser>(this IEndpointRouteBuilder endpoints)
        where TUser : class, new()
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        
        var emailSender = endpoints.ServiceProvider.GetRequiredService<IEmailSender<TUser>>();
        var linkGenerator = endpoints.ServiceProvider.GetRequiredService<LinkGenerator>();
        var stripeCredential = endpoints.ServiceProvider.GetRequiredService<IOptions<StripeCredential>>()?.Value;
        var stripeCredentialSigningSecret = stripeCredential?.SigningSecret;
        
        var jwtHelper = endpoints.ServiceProvider.GetRequiredService<JwtHelper>();
        //var jwtConfig = endpoints.ServiceProvider.GetRequiredService<IOptions<JwtConfig>>()?.Value;
        
        var routeGroup = endpoints.MapGroup("students");

        routeGroup.MapRegister<ApplicationUser>((IEmailSender<ApplicationUser>)emailSender, linkGenerator);
        routeGroup.MapForgotPassword((IEmailSender<ApplicationUser>)emailSender);
        routeGroup.MapResetPassword<ApplicationUser>();
        routeGroup.MapPayment<ApplicationUser>(stripeCredential);
        
        // routeGroup.MapLogin<ApplicationUser>(jwtHelper);
        // routeGroup.MapRefresh<ApplicationUser>(jwtHelper);
        //routeGroup.MapConfirmEmail<ApplicationUser>();
        //routeGroup.MapResendConfirmationEmail<TUser>(emailSender, linkGenerator);

        routeGroup.MapManageInfo();
        routeGroup.MapStateInfo();
        routeGroup.MapCoachInfo();
        routeGroup.MapCallInfo();
        routeGroup.MapOnboard();
        routeGroup.MapProgram();
        
        routeGroup.MapApprove();
        routeGroup.MapCancel();
        routeGroup.MapUploadProfilePhoto();
        routeGroup.MapGetProfilePhoto();
        
        routeGroup.MapUnsubscribe();

        return new IdentityEndpointsConventionBuilder(routeGroup).WithDescription("Onboard formunu submit et");
    }
    
}
