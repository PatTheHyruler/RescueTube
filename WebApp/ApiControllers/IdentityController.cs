using Asp.Versioning;
using AutoMapper;
using BLL.DTO.Exceptions.Identity;
using BLL.Identity.Services;
using Microsoft.AspNetCore.Mvc;
using Public.DTO.v1;
using Public.DTO.v1.Identity;
using Swashbuckle.AspNetCore.Annotations;
using WebApp.Helpers;

namespace WebApp.ApiControllers;

/// <summary>
/// API controller for user account-related endpoints
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("v{version:apiVersion}/[controller]/[action]")]
public class IdentityController : ControllerBase
{
    private readonly ILogger<IdentityController> _logger;
    private readonly IMapper _mapper;
    private readonly UserService _userService;
    private readonly TokenService _tokenService;
    private readonly Random _rnd = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityController"/> class.
    /// </summary>
    public IdentityController(ILogger<IdentityController> logger, IMapper mapper, UserService userService,
        TokenService tokenService)
    {
        _logger = logger;
        _mapper = mapper;
        _userService = userService;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    /// <param name="registrationData">Required data for registering a new user.</param>
    /// <returns>The created user, if successful.</returns>
    /// <response code="200">Registration was successful.</response>
    /// <response code="400">One or more identity errors occurred.</response>
    /// <response code="403">User account registration is currently disabled.</response>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(User))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, null, typeof(IEnumerable<IdentityRegistrationErrorResponse>))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, null, typeof(IEnumerable<IdentityRegistrationErrorResponse>))]
    public async Task<IActionResult> Register([FromBody] RegistrationData registrationData)
    {
        try
        {
            var user = await _userService.RegisterUserAsync(registrationData.Username, registrationData.Password);
            return Ok(_mapper.Map<User>(user));
        }
        catch (RegistrationDisabledException)
        {
            _logger.LogInformation("Registration attempted with username {UserName}, but registration is disabled",
                registrationData.Username);
            return StatusCode(StatusCodes.Status403Forbidden, new IdentityRegistrationErrorResponse
            {
                IdentityErrors = new IdentityRegistrationError[]
                {
                    new()
                    {
                        Code = EIdentityRegistrationErrorCode.RegistrationDisabled,
                        Description = "User account registration is currently disabled",
                    }
                }
            });
        }
        catch (IdentityOperationFailedException e)
        {
            return e.ToActionResult();
        }
    }

    /// <summary>
    /// Log in with a user account using password authentication.
    /// </summary>
    /// <param name="loginData">The credentials to use for logging in.</param>
    /// <returns>A new JSON Web Token for the user, and a token for refreshing the JWT.</returns>
    /// <response code="200">Authentication was successful.</response>
    /// <response code="404">Provided login credentials were invalid.</response>
    /// <response code="403">The provided login credentials were valid, but refer to an account that must be approved before it can be used..</response>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(JwtResponse))]
    [SwaggerResponse(StatusCodes.Status404NotFound, null, typeof(InvalidLoginResponse))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, null, typeof(RestApiErrorResponse))]
    public async Task<ActionResult<JwtResponse>> LogIn([FromBody] JwtLoginData loginData)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            var jwtResult = await _userService.SignInJwtAsync(loginData.Username, loginData.Password,
                loginData.ExpiresInSeconds);
            await _userService.SaveChangesAsync();
            return Ok(new JwtResponse
            {
                Jwt = jwtResult.Jwt,
                RefreshToken = jwtResult.RefreshToken.RefreshToken,
                RefreshTokenExpiresAt = jwtResult.RefreshToken.ExpiresAt,
            });
        }
        catch (UserNotFoundException)
        {
            await DelayRandomFromStart(startTime);
            return NotFound(new InvalidLoginResponse());
        }
        catch (WrongPasswordException)
        {
            await DelayRandomFromStart(startTime);
            return NotFound(new InvalidLoginResponse());
        }
        catch (UserNotApprovedException)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new RestApiErrorResponse
            {
                ErrorType = EErrorType.UserNotApproved,
                Message = "This user account must be approved by an administrator before it can be used.",
            });
        }
        catch (InvalidJwtExpirationRequestedException e)
        {
            return BadRequest(new RestApiErrorResponse
            {
                ErrorType = EErrorType.InvalidJwtExpirationRequested,
                Message = e.Message,
            });
        }
    }

    /// <summary>
    /// Get a new JWT and refresh token, using an existing JWT and refresh token.
    /// </summary>
    /// <param name="jwtRefreshTokenData">Data required for refreshing the tokens.</param>
    /// <returns>New JWT and refresh token.</returns>
    /// <response code="200">Token refresh was successful.</response>
    /// <response code="400">Provided JWT or refresh token was invalid.</response>
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(JwtResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, null, typeof(RestApiErrorResponse))]
    [HttpPost]
    public async Task<ActionResult<JwtResponse>> RefreshToken([FromBody] JwtRefreshData jwtRefreshTokenData)
    {
        try
        {
            var jwtResult = await _tokenService.RefreshTokenAsync(
                jwtRefreshTokenData.Jwt,
                jwtRefreshTokenData.RefreshToken,
                jwtRefreshTokenData.ExpiresInSeconds);
            await _tokenService.SaveChangesAsync();
            return Ok(new JwtResponse
            {
                Jwt = jwtResult.Jwt,
                RefreshToken = jwtResult.RefreshToken.RefreshToken,
                RefreshTokenExpiresAt = jwtResult.RefreshToken.ExpiresAt,
            });
        }
        catch (InvalidJwtException)
        {
            return BadRequest(new RestApiErrorResponse
            {
                ErrorType = EErrorType.InvalidJwt,
                Message = "Provided JWT was invalid.",
            });
        }
        catch (InvalidRefreshTokenException)
        {
            return BadRequest(new RestApiErrorResponse
            {
                ErrorType = EErrorType.InvalidRefreshToken,
                Message = "Invalid refresh token",
            });
        }
        catch (InvalidJwtExpirationRequestedException e)
        {
            return BadRequest(new RestApiErrorResponse
            {
                ErrorType = EErrorType.InvalidJwtExpirationRequested,
                Message = e.Message,
            });
        }
    }

    /// <summary>
    /// Log out user by deleting provided refresh token.
    /// </summary>
    /// <remarks>
    /// NB! JWT will remain valid until it expires.
    /// Client applications should forget it to complete logout after calling this method.
    /// </remarks>
    /// <param name="jwtLogoutData"></param>
    /// <response code="200">Refresh token deleted successfully.</response>
    /// <response code="400">Invalid JWT provided.</response>
    [SwaggerResponse(StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(RestApiErrorResponse))]
    [HttpDelete]
    public async Task<IActionResult> LogOut([FromBody] JwtLogoutData jwtLogoutData)
    {
        try
        {
            await _tokenService.DeleteRefreshTokenAsync(jwtLogoutData.Jwt, jwtLogoutData.RefreshToken);
            return Ok();
        }
        catch (InvalidJwtException)
        {
            return BadRequest(new RestApiErrorResponse
            {
                ErrorType = EErrorType.InvalidJwt,
                Message = "Provided JWT was invalid",
            });
        }
    }

    private async Task DelayRandomFromStart(DateTime startTime, int desiredMaxValueMs = 1000, int minValueMs = 100)
    {
        var msDiff = Convert.ToInt32(DateTime.UtcNow.Subtract(startTime).TotalMilliseconds);
        var actualMax = desiredMaxValueMs - msDiff;
        if (actualMax <= 0) return;
        await DelayRandom(minValueMs: minValueMs, maxValueMs: actualMax);
    }

    private async Task DelayRandom(int minValueMs = 100, int maxValueMs = 1000)
    {
        minValueMs = Math.Max(0, minValueMs);
        maxValueMs = Math.Max(minValueMs, maxValueMs);
        await Task.Delay(_rnd.Next(minValueMs, maxValueMs));
    }
}