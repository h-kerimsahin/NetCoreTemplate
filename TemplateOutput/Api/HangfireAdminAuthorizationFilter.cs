using Hangfire.Dashboard;

namespace $safeprojectname$.Api;

public class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext ctx)
    {
        var http = ctx.GetHttpContext();
        var user = http.User;
        return user.IsInRole("SuperAdmin") || user.IsInRole("Admin");
    }
}
