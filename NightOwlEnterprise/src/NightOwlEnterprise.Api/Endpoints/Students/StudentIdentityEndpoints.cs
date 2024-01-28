using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public static class StudentIdentityEndpoints
{
    public static IEndpointConventionBuilder MapStudentsIdentityApi<TUser>(this IEndpointRouteBuilder endpoints)
        where TUser : class, new()
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        
        var emailSender = endpoints.ServiceProvider.GetRequiredService<IEmailSender<TUser>>();
        var linkGenerator = endpoints.ServiceProvider.GetRequiredService<LinkGenerator>();

        var routeGroup = endpoints.MapGroup("students");

        routeGroup.MapRegister<StudentApplicationUser>((IEmailSender<StudentApplicationUser>)emailSender,
            linkGenerator);
        routeGroup.MapLogin<StudentApplicationUser>();
        routeGroup.MapRefresh<StudentApplicationUser>();
        routeGroup.MapConfirmEmail<StudentApplicationUser>();
        routeGroup.MapResendConfirmationEmail<TUser>(emailSender, linkGenerator);
        routeGroup.MapForgotPassword<TUser>(emailSender);
        routeGroup.MapResetPassword<TUser>();
        routeGroup.MapManageInfo<TUser>();

        return new IdentityEndpointsConventionBuilder(routeGroup);
    }

    // Wrap RouteGroupBuilder with a non-public type to avoid a potential future behavioral breaking change.
    private sealed class IdentityEndpointsConventionBuilder : IEndpointConventionBuilder
    {
        private readonly IEndpointConventionBuilder _innerAsConventionBuilder;

        public IdentityEndpointsConventionBuilder(IEndpointConventionBuilder inner)
        {
            _innerAsConventionBuilder = inner ?? throw new ArgumentNullException(nameof(inner));
        }
        
        public void Add(Action<EndpointBuilder> convention) => _innerAsConventionBuilder.Add(convention);
        public void Finally(Action<EndpointBuilder> finallyConvention) => _innerAsConventionBuilder.Finally(finallyConvention);
    }
    
}
