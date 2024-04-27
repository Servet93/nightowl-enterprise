using NightOwlEnterprise.Api.Endpoints.Invitations.Coach;
using NightOwlEnterprise.Api.Endpoints.Invitations.Student;

namespace NightOwlEnterprise.Api.Endpoints.Invitations;

public static class InvitationsEndpoints
{
    public static IEndpointConventionBuilder MapInvitationsApi(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        
        var routeGroup = endpoints.MapGroup("invitations");
        
        routeGroup.MapInviteOperationForCoach();
        routeGroup.MapInviteOperationForStudent();

        return new IdentityEndpointsConventionBuilder(routeGroup);
    }
}
