using System.Security.Claims;
using Asp.Versioning;
using HeyRed.Mime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RescueTube.Core;
using RescueTube.Core.Identity.Exceptions;
using RescueTube.Core.Identity.Services;
using RescueTube.Core.Services;
using RescueTube.Core.Utils;
using Swashbuckle.AspNetCore.Annotations;
using WebApp.ApiModels;
using WebApp.ApiModels.Auth;
using WebApp.Auth;
using WebApp.Utils;

namespace WebApp.ApiControllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/videos/{videoId:guid}/file")]
public class VideoFileController : ControllerBase
{
    private readonly AppPathOptions _appPathOptions;
    private readonly TokenService _tokenService;
    private readonly AuthorizationService _authorizationService;
    private readonly VideoPresentationService _videoPresentationService;
    private readonly IWebHostEnvironment _environment;

    public VideoFileController(TokenService tokenService, AuthorizationService authorizationService,
        VideoPresentationService videoPresentationService, IWebHostEnvironment environment,
        IOptions<AppPathOptions> appPathOptions)
    {
        _tokenService = tokenService;
        _authorizationService = authorizationService;
        _videoPresentationService = videoPresentationService;
        _environment = environment;
        _appPathOptions = appPathOptions.Value;
    }

    /// <summary>
    /// Get the file for a video.
    /// </summary>
    /// <param name="videoId">Identifier for the video.</param>
    /// <returns>The file data.</returns>
    /// <response code="200">File data fetched successfully.</response>
    /// <response code="403">Access to video file forbidden.</response>
    /// <response code="404">File (or video) not found.</response>
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(FileResult))]
    [SwaggerResponse(StatusCodes.Status403Forbidden)]
    [SwaggerErrorResponse(StatusCodes.Status404NotFound)]
    [HttpGet("data")]
    public async Task<IResult> FileAsync(Guid videoId)
    {
        var requestToken = HttpContext.Request.Cookies[GetVideoFileAccessCookieName(videoId)];
        var tokenValidated = false;
        if (requestToken != null)
        {
            try
            {
                var requestClaims = _tokenService.ValidateJwt(requestToken, ignoreExpiration: false,
                    audienceSuffix: GetAudienceSuffix(videoId)
                );
                tokenValidated = true;
                var (token, expiresAt) = CreateVideoAccessToken(videoId, requestClaims);
                SetResponseVideoAccessToken(videoId, token, expiresAt);
            }
            catch (InvalidJwtException)
            {
                ClearVideoAccessToken(videoId);
            }
        }

        if (!tokenValidated && !await _authorizationService.IsVideoAccessAllowed(videoId))
        {
            return Results.Forbid();
        }

        return await VideoFileInternalAsync(videoId);
    }

    /// <summary>
    /// Creates and gets a short-lived access token for accessing the file of a video.
    /// </summary>
    /// <param name="videoId">Identifier for the video whose file to get an access token for.</param>
    /// <returns>The token (also sets the token to a cookie).</returns>
    /// <response code="200">Token created successfully.</response>
    /// <response code="403">Authenticated user doesn't have permission to access the video or video not found.</response>
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(AccessTokenDtoV1))]
    [SwaggerResponse(StatusCodes.Status403Forbidden)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [EnableCors(AuthHelpers.CorsPolicies.CorsAllowCredentials)]
    [HttpGet("accessToken")]
    public async Task<ActionResult<AccessTokenDtoV1>> GetNewVideoAccessToken([FromRoute] Guid videoId)
    {
        if (!await _authorizationService.IsVideoAccessAllowed(videoId, User))
        {
            return Forbid();
        }

        var (token, expiresAt) = CreateVideoAccessToken(videoId, User);
        SetResponseVideoAccessToken(videoId, token, expiresAt);
        return Ok(new AccessTokenDtoV1
        {
            Token = token,
            ExpiresAt = expiresAt,
        });
    }

    private async Task<IResult> VideoFileInternalAsync(Guid videoId)
    {
        var videoFile = await _videoPresentationService.GetVideoFileAsync(videoId);
        if (videoFile == null)
        {
            return VideoFileNotFound(videoId);
        }

        var filePath = videoFile.FilePath;

        var contentType = MimeTypesMap.GetMimeType(filePath);

        FileStream stream;
        try
        {
            // Using ContentRootPath is necessary on some OSes / hosting scenarios?
            stream = System.IO.File.OpenRead(Path.Combine(
                _environment.ContentRootPath,
                _appPathOptions.Downloads,
                filePath
            ));
        }
        catch (FileNotFoundException)
        {
            return VideoFileNotFound(videoId);
        }
        catch (DirectoryNotFoundException)
        {
            return VideoFileNotFound(videoId);
        }

        return Results.File(stream, contentType, enableRangeProcessing: true);
    }

    private const string VideoFileAccessTokenCookieName = "VideoFileAccessToken";
    private static string GetVideoFileAccessCookieName(Guid videoId) => VideoFileAccessTokenCookieName + videoId;
    private static string GetAudienceSuffix(Guid videoId) => $"/Videos/File/{videoId}";
    private const int ExpiresInSeconds = 60;

    private (string Token, DateTimeOffset ExpiresAt) CreateVideoAccessToken(Guid videoId, ClaimsPrincipal claims)
    {
        var token = _tokenService.GenerateJwt(claims, expiresInSeconds: ExpiresInSeconds,
            audienceSuffix: GetAudienceSuffix(videoId));
        return (token, DateTimeOffset.UtcNow.AddSeconds(ExpiresInSeconds));
    }

    private void SetResponseVideoAccessToken(Guid videoId, string token, DateTimeOffset expiresAt)
    {
        Response.Cookies.Append(
            GetVideoFileAccessCookieName(videoId),
            token,
            new CookieOptions
            {
                Expires = expiresAt, IsEssential = true, SameSite = SameSiteMode.None, Secure = true,
                HttpOnly = true,
                Extensions = { "Partitioned", },
            });
    }

    private void ClearVideoAccessToken(Guid videoId)
    {
        Response.Cookies.Delete(GetVideoFileAccessCookieName(videoId));
    }

    private static IResult VideoFileNotFound(Guid videoId) => Results.NotFound(new ErrorResponseDto
    {
        ErrorType = EErrorType.EntityNotFound,
        Message = $"Video file for video {videoId} not found",
    });
}