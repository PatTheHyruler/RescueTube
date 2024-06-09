using System.Security.Claims;
using RescueTube.Core.Identity.Services;
using Hangfire.Dashboard;
using Microsoft.Extensions.Primitives;
using RescueTube.Core.DTO.Entities.Identity;
using RescueTube.Core.Identity;

namespace WebApp.Auth;

public class HangfireDashboardAuthorizationFilter : IDashboardAsyncAuthorizationFilter
{
    private readonly IServiceProvider _serviceProvider;

    private const string CookieName = "RescueTube-Hangfire-Token";

    public HangfireDashboardAuthorizationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<bool> AuthorizeAsync(DashboardContext context)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var tokenService = services.GetRequiredService<TokenService>();

        var query = ParseQuery(context);
        if (query != null)
        {
            var decodedJwt = GetPrincipal(tokenService, query.Jwt);
            if (decodedJwt == null)
            {
                return false;
            }

            var newJwt = GenerateJwt(tokenService, decodedJwt.Principal);
            SetCookie(context.GetHttpContext().Response, newJwt);
            context.GetHttpContext().Response.Redirect(query.HangfireUrl);
            return true;
        }

        var cookieJwt = context.GetHttpContext().Request.Cookies[CookieName];
        if (cookieJwt == null)
        {
            return false;
        }

        var decodedCookieJwt = GetPrincipal(tokenService, cookieJwt);
        if (decodedCookieJwt == null || !decodedCookieJwt.Principal.IsAdmin())
        {
            return false;
        }

        // Renew JWT
        if (decodedCookieJwt.SecurityToken.ValidTo.ToUniversalTime() < DateTimeOffset.UtcNow.AddMinutes(1))
        {
            var identityUow = services.GetRequiredService<IdentityUow>();
            var user = await identityUow.UserManager.FindByIdAsync(decodedCookieJwt.Principal.GetUserId().ToString());
            if (user == null)
            {
                return false;
            }

            var newPrincipal = await identityUow.SignInManager.CreateUserPrincipalAsync(user);
            if (!newPrincipal.IsAdmin())
            {
                return false;
            }

            var newJwt = GenerateJwt(tokenService, newPrincipal);
            SetCookie(context.GetHttpContext().Response, newJwt);
        }

        return true;
    }

    private record InitialQuery(string Jwt, string HangfireUrl);

    private static InitialQuery? ParseQuery(DashboardContext context)
    {
        var request = context.GetHttpContext().Request;

        var tokenValues = request.Query["HangfireToken"];
        var token = StringValues.IsNullOrEmpty(tokenValues) ? null : tokenValues[0];

        var hangfireUrlValues = request.Query["HangfireUrl"];
        var hangfireUrl = StringValues.IsNullOrEmpty(hangfireUrlValues) ? null : hangfireUrlValues[0];

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(hangfireUrl))
        {
            return null;
        }

        return new InitialQuery(Jwt: token, HangfireUrl: hangfireUrl);
    }

    private static DecodedJwt? GetPrincipal(TokenService tokenService, string jwt)
    {
        try
        {
            var decodedJwt =
                tokenService.ValidateJwt(jwt, ignoreExpiration: false, RescueTubeIdentity.HangfireJwtSuffix);
            return !decodedJwt.Principal.IsAdmin() ? null : decodedJwt;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string GenerateJwt(TokenService tokenService, ClaimsPrincipal principal)
    {
        return tokenService.GenerateJwt(principal, expiresInSeconds: 5 * 60, RescueTubeIdentity.HangfireJwtSuffix);
    }

    private static void SetCookie(HttpResponse response, string value)
    {
        response.Cookies.Delete(CookieName);
        response.Cookies.Append(CookieName, value, new CookieOptions
        {
            IsEssential = true,
            Secure = true,
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddMinutes(5),
        });
    }
}