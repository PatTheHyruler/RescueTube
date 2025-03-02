using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using RescueTube.Core.DTO.Entities.Identity;
using RescueTube.Core.Identity;
using RescueTube.Core.Identity.Services;

namespace WebApp.Auth;

public class HangfireAuthService
{
    public const string AuthenticationScheme = "HangfireCookieToken";

    private readonly IdentityUow _identityUow;
    private readonly TimeProvider _timeProvider;

    private const string TokenCookieName = "RescueTube-Hangfire-Token";
    private const string AppAuthUrlCookieName = "RescueTube-Hangfire-Redirect-AppAuthUrl";

    public HangfireAuthService(IdentityUow identityUow, TimeProvider timeProvider)
    {
        _identityUow = identityUow;
        _timeProvider = timeProvider;
    }

    public record InitialAuthQuery(string HangfireJwt, string TargetUrl, string? AppAuthUrl);

    public IResult HandleInitialAuth(InitialAuthQuery query, HttpResponse response)
    {
        DecodedJwt decodedJwt;
        try
        {
            decodedJwt = _identityUow.TokenService.ValidateJwt(query.HangfireJwt, ignoreExpiration: false, RescueTubeIdentity.HangfireJwtSuffix);
        }
        catch (Exception)
        {
            return Results.BadRequest("Token validation failed");
        }

        var newJwt = GenerateJwt(decodedJwt.Principal);
        SetTokenCookie(response, newJwt);
        if (!string.IsNullOrWhiteSpace(query.AppAuthUrl))
        {
            response.Cookies.Delete(AppAuthUrlCookieName);
            response.Cookies.Append(AppAuthUrlCookieName, query.AppAuthUrl, new CookieOptions
            {
                IsEssential = true,
                SameSite = SameSiteMode.Strict,
            });
        }

        return Results.Redirect(query.TargetUrl);
    }

    public async Task<AuthenticateResult> HandleAuthenticateAsync(HttpRequest request, HttpResponse response)
    {
        var cookieJwt = request.Cookies[TokenCookieName];
        if (string.IsNullOrWhiteSpace(cookieJwt))
        {
            return AuthenticateResult.NoResult();
        }

        DecodedJwt decodedJwt;
        try
        {
            decodedJwt = _identityUow.TokenService.ValidateJwt(cookieJwt, ignoreExpiration: false, RescueTubeIdentity.HangfireJwtSuffix);
        }
        catch (Exception)
        {
            if (request.Cookies.TryGetValue(AppAuthUrlCookieName, out var appUrl) && !string.IsNullOrWhiteSpace(appUrl))
            {
                response.Redirect(appUrl);
            }
            return AuthenticateResult.Fail("Token validation failed");
        }

        var userId = decodedJwt.Principal.GetUserIdIfExists();
        if (userId is null)
        {
            return AuthenticateResult.Fail("User ID missing from token");
        }

        var refreshedPrincipal = await TryRefreshTokenAndGetNewPrincipalAsync(response, decodedJwt, userId.Value);

        return AuthenticateResult.Success(new AuthenticationTicket(refreshedPrincipal ?? decodedJwt.Principal, AuthenticationScheme));
    }

    private async Task<ClaimsPrincipal?> TryRefreshTokenAndGetNewPrincipalAsync(
        HttpResponse response, DecodedJwt decodedJwt, Guid userId)
    {
        if (decodedJwt.SecurityToken.ValidTo.ToUniversalTime() >= _timeProvider.GetUtcNow().AddMinutes(1))
        {
            return null;
        }

        var user = await _identityUow.UserManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return null;
        }

        var newPrincipal = await _identityUow.SignInManager.CreateUserPrincipalAsync(user);
        var newJwt = GenerateJwt(newPrincipal);
        SetTokenCookie(response, newJwt);

        return newPrincipal;
    }

    private string GenerateJwt(ClaimsPrincipal principal)
    {
        return _identityUow.TokenService.GenerateJwt(principal);
    }

    private static void SetTokenCookie(HttpResponse response, string value)
    {
        response.Cookies.Delete(TokenCookieName);
        response.Cookies.Append(TokenCookieName, value, new CookieOptions
        {
            IsEssential = true,
            Secure = true,
            HttpOnly = true,
            Expires = DateTimeOffset.UtcNow.AddMinutes(5),
        });
    }

    public bool TryRedirectToAppUrl(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue(AppAuthUrlCookieName, out var appUrl) && !string.IsNullOrWhiteSpace(appUrl))
        {
            var redirectUrl = QueryHelpers.AddQueryString(appUrl, "url", context.Request.GetDisplayUrl());
            context.Response.Redirect(redirectUrl);
            return true;
        }

        return false;
    }
}