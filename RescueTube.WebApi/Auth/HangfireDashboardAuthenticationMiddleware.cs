namespace RescueTube.WebApi.Auth;

public class HangfireDashboardAuthenticationMiddleware : IMiddleware
{
    private readonly HangfireAuthService _hangfireAuthService;

    public HangfireDashboardAuthenticationMiddleware(HangfireAuthService hangfireAuthService)
    {
        _hangfireAuthService = hangfireAuthService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var result = await _hangfireAuthService.HandleAuthenticateAsync(context.Request, context.Response);
        if (!result.Succeeded)
        {
            var successfullyRedirected = _hangfireAuthService.TryRedirectToAppUrl(context);
            if (!successfullyRedirected)
            {
                context.Response.StatusCode = 401;
            }
            return;
        }
        context.User = result.Principal;

        await next(context);
    }
}