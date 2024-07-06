using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NightOwlEnterprise.Api.Endpoints.Common;
using NightOwlEnterprise.Api.Entities;

namespace NightOwlEnterprise.Api.Endpoints.Tests;

public static class TestsEndpoints
{
    public static IEndpointConventionBuilder MapTestsApi(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        
        var routeGroup = endpoints.MapGroup("/tests").WithOpenApi().WithTags("Tests");

        routeGroup.MapStartVoiceCalls();
        routeGroup.MapSystemMessage();
        
        return new IdentityEndpointsConventionBuilder(routeGroup);
    }
    
}
