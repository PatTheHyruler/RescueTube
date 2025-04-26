using System.Security.Claims;
using HeyRed.Mime;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RescueTube.Core.Identity.Exceptions;
using RescueTube.Core.Identity.Services;
using RescueTube.Core.Services;
using RescueTube.Core.Utils;
using WebApp.ApiModels;
using WebApp.ApiModels.Auth;
using WebApp.Auth;

namespace WebApp.Endpoints;

public static class VideoFileEndpoints
{
    public static void MapVideoFileEndpoints(this IEndpointRouteBuilder app)
    {
        var videoFileGroup = app.MapGroup("videos/{videoId:guid}/file").WithTags("VideoFile");

        videoFileGroup.MapGet("data", ServeVideoFileAsync)
            .AllowAnonymous()
            .HasApiVersion(1);

        videoFileGroup.MapGet("AccessToken", GetNewVideoAccessTokenAsync)
            .RequireCors(AuthHelpers.CorsPolicies.CorsAllowCredentials)
            .HasApiVersion(1);
    }

    /// <summary>
    /// Get the file for a video.
    /// </summary>
    /// <returns>The file data.</returns>
    /// <response code="200">File data fetched successfully.</response>
    /// <response code="403">Access to video file forbidden.</response>
    /// <response code="404">File (or video) not found.</response>
    private static async Task<Results<
        FileStreamHttpResult, NotFound<ErrorResponseDto>, ForbidHttpResult
    >> ServeVideoFileAsync(
        [FromRoute] Guid videoId,
        [FromServices] TokenService tokenService,
        [FromServices] AuthorizationService authorizationService,
        [FromServices] VideoPresentationService videoPresentationService,
        [FromServices] IWebHostEnvironment environment,
        [FromServices] IOptions<AppPathOptions> appPathOptions,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var requestToken = httpContext.Request.Cookies[GetVideoFileAccessCookieName(videoId)];
        var tokenValidated = false;
        if (requestToken != null)
        {
            try
            {
                var requestClaims = tokenService.ValidateJwtGetPrincipal(requestToken, ignoreExpiration: false,
                    audienceSuffix: GetAudienceSuffix(videoId)
                );
                tokenValidated = true;
                var (token, expiresAt) = CreateVideoAccessTokenAsync(tokenService, videoId, requestClaims);
                SetResponseVideoAccessToken(httpContext.Response, videoId, token, expiresAt);
            }
            catch (InvalidJwtException)
            {
                httpContext.Response.Cookies.Delete(GetVideoFileAccessCookieName(videoId));
            }
        }

        if (!tokenValidated && !await authorizationService.IsVideoAccessAllowedAsync(videoId, ct: ct))
        {
            return TypedResults.Forbid();
        }

        var videoFile = await videoPresentationService.GetVideoFileAsync(videoId, ct);
        if (videoFile == null)
        {
            return VideoFileNotFound(videoId);
        }

        var filePath = videoFile.FilePath;

        var contentType = MimeTypesMap.GetMimeType(filePath);

        try
        {
            // Using ContentRootPath is necessary on some OSes / hosting scenarios?
            var stream = File.OpenRead(Path.Combine(
                environment.ContentRootPath,
                appPathOptions.Value.Downloads,
                filePath
            ));

            return TypedResults.File(stream, contentType, enableRangeProcessing: true);
        }
        catch (FileNotFoundException)
        {
            return VideoFileNotFound(videoId);
        }
        catch (DirectoryNotFoundException)
        {
            return VideoFileNotFound(videoId);
        }
    }

    /// <summary>
    /// Creates and gets a short-lived access token for accessing the file of a video.
    /// </summary>
    /// <returns>The token (also sets the token to a cookie).</returns>
    /// <response code="200">Token created successfully.</response>
    /// <response code="403">Authenticated user doesn't have permission to access the video or video not found.</response>
    private static async Task<Results<Ok<AccessTokenDtoV1>, ForbidHttpResult>> GetNewVideoAccessTokenAsync(
        [FromRoute] Guid videoId,
        [FromServices] AuthorizationService authorizationService,
        [FromServices] TokenService tokenService,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (!await authorizationService.IsVideoAccessAllowedAsync(videoId, httpContext.User, ct))
        {
            return TypedResults.Forbid();
        }

        var (token, expiresAt) = CreateVideoAccessTokenAsync(tokenService, videoId, httpContext.User);

        SetResponseVideoAccessToken(httpContext.Response, videoId, token, expiresAt);
        return TypedResults.Ok(new AccessTokenDtoV1
        {
            Token = token,
            ExpiresAt = expiresAt,
        });
    }

    private const string VideoFileAccessTokenCookieName = "VideoFileAccessToken";
    private static string GetVideoFileAccessCookieName(Guid videoId) => VideoFileAccessTokenCookieName + videoId;
    private static string GetAudienceSuffix(Guid videoId) => $"/Videos/File/{videoId}";
    private const int ExpiresInSeconds = 60;

    private static (string Token, DateTimeOffset ExpiresAt) CreateVideoAccessTokenAsync(TokenService tokenService, Guid videoId, ClaimsPrincipal claims)
    {
        var token = tokenService.GenerateJwt(
            claims.Claims.Where(c => c.Type != "aud"),
            expiresInSeconds: ExpiresInSeconds,
            audienceSuffix: GetAudienceSuffix(videoId));
        return (token, DateTimeOffset.UtcNow.AddSeconds(ExpiresInSeconds));
    }

    private static void SetResponseVideoAccessToken(HttpResponse httpResponse, Guid videoId, string token, DateTimeOffset expiresAt)
    {
        httpResponse.Cookies.Append(
            GetVideoFileAccessCookieName(videoId),
            token,
            new CookieOptions
            {
                Expires = expiresAt, IsEssential = true, SameSite = SameSiteMode.None, Secure = true,
                HttpOnly = true,
                Extensions = { "Partitioned", },
            });
    }

    private static NotFound<ErrorResponseDto> VideoFileNotFound(Guid videoId) => TypedResults.NotFound(
        new ErrorResponseDto
        {
            ErrorType = EErrorType.EntityNotFound,
            Message = $"Video file for video {videoId} not found",
        });
}