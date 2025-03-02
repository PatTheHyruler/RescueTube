using RescueTube.Core.Identity.Services;
using Hangfire.Dashboard;

namespace WebApp.Auth;

public class HangfireDashboardAuthorizationFilter : IDashboardAsyncAuthorizationFilter
{
    public Task<bool> AuthorizeAsync(DashboardContext context)
    {
        return Task.FromResult(context.GetHttpContext().User.IsAdmin());
    }
}