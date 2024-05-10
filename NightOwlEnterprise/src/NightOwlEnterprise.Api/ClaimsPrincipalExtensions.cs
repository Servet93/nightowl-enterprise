using System.Security.Claims;

namespace NightOwlEnterprise.Api;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetId(this ClaimsPrincipal claimsPrincipal)
    {
        var idStr = claimsPrincipal?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        Guid.TryParse(idStr, out var id);

        return id;
    }
}