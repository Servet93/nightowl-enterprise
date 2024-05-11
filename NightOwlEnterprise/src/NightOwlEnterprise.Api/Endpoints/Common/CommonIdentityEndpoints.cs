using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NightOwlEnterprise.Api.Endpoints.Common;

namespace NightOwlEnterprise.Api.Endpoints.Coachs;

public static class CommonIdentityEndpoints
{
    public static IEndpointConventionBuilder MapCommonIdentityApi(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        
        var emailSender = endpoints.ServiceProvider.GetRequiredService<IEmailSender<ApplicationUser>>();
        var linkGenerator = endpoints.ServiceProvider.GetRequiredService<LinkGenerator>();
        var stripeCredential = endpoints.ServiceProvider.GetRequiredService<IOptions<StripeCredential>>()?.Value;
        var stripeCredentialSigningSecret = stripeCredential?.SigningSecret;
        
        var jwtHelper = endpoints.ServiceProvider.GetRequiredService<JwtHelper>();
        //var jwtConfig = endpoints.ServiceProvider.GetRequiredService<IOptions<JwtConfig>>()?.Value;
        
        var routeGroup = endpoints.MapGroup("").WithOpenApi().WithTags("Oturum");

        routeGroup.MapLogin(jwtHelper);
        // routeGroup.MapRefresh(jwtHelper);
        
        // routeGroup.MapGet("/special-student", Results<Ok<string>, ProblemHttpResult>
        //     () => TypedResults.Ok("Special Accessed With Student")).RequireAuthorization("Student");
        //
        // routeGroup.MapGet("/special-coach", Results<Ok<string>, ProblemHttpResult>
        //     () => TypedResults.Ok("Special Accessed With Coach")).RequireAuthorization("Coach");
        
        return new IdentityEndpointsConventionBuilder(routeGroup);
    }
    
}
