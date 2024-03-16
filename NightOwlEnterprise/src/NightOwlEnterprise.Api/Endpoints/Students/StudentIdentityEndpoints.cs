using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

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
        
        var routeGroup = endpoints.MapGroup("students");

        routeGroup.MapRegister<ApplicationUser>((IEmailSender<ApplicationUser>)emailSender,
            linkGenerator);
        routeGroup.MapPayment<ApplicationUser>(stripeCredentialSigningSecret);
        routeGroup.MapLogin<ApplicationUser>();
        routeGroup.MapRefresh<ApplicationUser>();
        routeGroup.MapConfirmEmail<ApplicationUser>();
        routeGroup.MapResendConfirmationEmail<TUser>(emailSender, linkGenerator);
        routeGroup.MapForgotPassword<TUser>(emailSender);
        routeGroup.MapResetPassword<TUser>();
        routeGroup.MapManageInfo<TUser>();

        return new IdentityEndpointsConventionBuilder(routeGroup);
    }


    
}
