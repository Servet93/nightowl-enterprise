namespace NightOwlEnterprise.Api.Endpoints.Invitations.Student;

public static class InvitationEndpoints
{
    public static void MapInviteOperationForStudent(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapApprove();
        endpoints.MapCancel();
    }
}