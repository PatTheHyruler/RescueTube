using BLL.Identity.Services;
using Hangfire.Dashboard;

namespace WebApp.Auth;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var user = context.GetHttpContext().User;
        return user.IsAdmin();
    }
}