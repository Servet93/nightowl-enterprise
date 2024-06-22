using Hangfire.Dashboard;

namespace NightOwlEnterprise.Api;

public class DashboardNoAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}