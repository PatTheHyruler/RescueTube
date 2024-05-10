using Asp.Versioning;
using BLL.DTO.Exceptions.Identity;
using BLL.Identity;
using BLL.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApp.ApiModels;
using WebApp.ApiModels.Auth;
using WebApp.Auth;
using WebApp.Utils;

namespace WebApp.ApiControllers;

/// <summary>
/// API controller for user account management endpoints
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class AccountController : ControllerBase
{
    private readonly IdentityUow _identityUow;

    public AccountController(IdentityUow identityUow)
    {
        _identityUow = identityUow;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="registrationData">Required data for registering a new user.</param>
    /// <returns>New JWT and refresh token (with expiration date) for the registered account, if registration doesn't require further approval.</returns>
    /// <response code="200">The registration was successful.</response>
    /// <response code="400">User with provided username is already registered or provided registration data was invalid.</response>
    /// <response code="202">The registration was successful, but must be approved by an administrator before the account can be used.</response>
    [SwaggerErrorResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status202Accepted)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(JwtResponseDtoV1))]
    [HttpPost]
    public async Task<ActionResult<JwtResponseDtoV1>> Register([FromBody] RegisterDtoV1 registrationData)
    {
        try
        {
            await _identityUow.UserService.RegisterUserAsync(
                registrationData.Username,
                registrationData.Password);
            await _identityUow.SaveChangesAsync();

            var jwtResult =
                await _identityUow.UserService.SignInJwtAsync(registrationData.Username, registrationData.Password);
            await _identityUow.SaveChangesAsync();

            return new JwtResponseDtoV1
            {
                Jwt = jwtResult.Jwt,
                RefreshToken = jwtResult.RefreshToken.Token,
                RefreshTokenExpiresAt = jwtResult.RefreshToken.ExpiresAt,
            };
        }
        catch (IdentityOperationFailedException e)
        {
            return BadRequest(new ErrorResponseDto
            {
                ErrorType = EErrorType.InvalidRegistrationData,
                Message = "Invalid registration data",
                Details = new
                {
                    IdentityErrors = e.Errors // TODO: map these to SubErrors, add details if necessary
                        .Where(error => AuthHelpers.AllowedPasswordErrors.Contains(error.Code) ||
                                        AuthHelpers.AllowedRegisterUsernameErrors.Contains(error.Code)),
                },
            });
        }
        catch (RegistrationDisabledException)
        {
            return BadRequest(new ErrorResponseDto
            {
                ErrorType = EErrorType.RegistrationDisabled,
                Message = "Registering new accounts is currently disabled",
            });
        }
    }

    /// <summary>
    /// Log in as an existing user, using password authentication
    /// </summary>
    /// <param name="loginData">Required data for logging in</param>
    /// <returns>New JWT and refresh token (with expiration date), if login was successful.</returns>
    /// <response code="200">Login was successful.</response>
    /// <response code="400">Username or password was invalid.</response>
    /// <response code="401">Login isn't allowed, because the user account hasn't been approved yet.</response>
    [SwaggerErrorResponse(StatusCodes.Status400BadRequest)]
    [SwaggerErrorResponse(StatusCodes.Status404NotFound)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(JwtResponseDtoV1))]
    [HttpPost]
    public async Task<ActionResult<JwtResponseDtoV1>> Login([FromBody] LoginDtoV1 loginData)
    {
        try
        {
            var jwtResult = await _identityUow.UserService.SignInJwtAsync(loginData.Username, loginData.Password);
            await _identityUow.SaveChangesAsync();
            return new JwtResponseDtoV1
            {
                Jwt = jwtResult.Jwt,
                RefreshToken = jwtResult.RefreshToken.Token,
                RefreshTokenExpiresAt = jwtResult.RefreshToken.ExpiresAt,
            };
        }
        catch (Exception e)
        {
            ActionResult? result = e switch
            {
                UserNotFoundException => BadRequest(new ErrorResponseDto
                {
                    ErrorType = EErrorType.InvalidLoginCredentials,
                }),
                WrongPasswordException => BadRequest(new ErrorResponseDto
                {
                    ErrorType = EErrorType.InvalidLoginCredentials,
                }),
                UserNotApprovedException => Unauthorized(new ErrorResponseDto
                {
                    ErrorType = EErrorType.UserNotApproved,
                }),
                _ => null,
            };
            if (result != null)
            {
                return result;
            }

            throw;
        }
    }

    /// <summary>
    /// Get a new JWT and refresh token, using existing JWT and refresh token.
    /// </summary>
    /// <param name="refreshTokenModel">Tokens to refresh</param>
    /// <returns>Refreshed JWT and refresh token (with expiration date), if refreshing was successful.</returns>
    /// <response code="200">Token refresh was successful.</response>
    /// <response code="400">Provided token/tokens was/were invalid.</response>
    [SwaggerErrorResponse(StatusCodes.Status400BadRequest)]
    [SwaggerErrorResponse(StatusCodes.Status404NotFound)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(JwtResponseDtoV1))]
    [HttpPost]
    public async Task<ActionResult<JwtResponseDtoV1>> RefreshToken(
        [FromBody] RefreshTokenRequestDtoV1 refreshTokenModel)
    {
        try
        {
            var jwtResult = await _identityUow.TokenService.RefreshTokenAsync(refreshTokenModel.Jwt,
                refreshTokenModel.RefreshToken);
            await _identityUow.SaveChangesAsync();
            return new JwtResponseDtoV1
            {
                Jwt = jwtResult.Jwt,
                RefreshToken = jwtResult.RefreshToken.Token,
                RefreshTokenExpiresAt = jwtResult.RefreshToken.ExpiresAt,
            };
        }
        catch (InvalidJwtException)
        {
            return BadRequest(new ErrorResponseDto
            {
                ErrorType = EErrorType.InvalidJwt,
            });
        }
        catch (InvalidRefreshTokenException)
        {
            return BadRequest(new ErrorResponseDto
            {
                ErrorType = EErrorType.InvalidRefreshToken,
                Message = "Invalid refresh token (probably expired)",
            });
        }
    }

    /// <summary>
    /// Log out user by deleting provided refresh token.
    /// User access will be refused when JWT expires.
    /// </summary>
    /// <param name="logout">The refresh token to delete.</param>
    /// <response code="200">Refresh token deleted successfully.</response>
    /// <response code="400">Invalid JWT provided.</response>
    [SwaggerErrorResponse(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<ActionResult> Logout(
        [FromBody] LogoutDtoV1 logout)
    {
        try
        {
            await _identityUow.TokenService.DeleteRefreshTokenAsync(logout.Jwt, logout.RefreshToken);
            await _identityUow.SaveChangesAsync();
            return Ok();
        }
        catch (InvalidJwtException)
        {
            return BadRequest(new ErrorResponseDto
            {
                ErrorType = EErrorType.InvalidJwt,
                Message = "Provided JWT was invalid",
            });
        }
    }

    /// <summary>
    /// Get information about the authenticated user.
    /// </summary>
    /// <response code="200">User information fetched successfully</response>
    /// <response code="404">User not found</response>
    [SwaggerErrorResponse(StatusCodes.Status404NotFound)]
    [SwaggerResponse(StatusCodes.Status200OK, null, typeof(MeResponseDtoV1))]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet]
    public async Task<ActionResult<MeResponseDtoV1>> Me()
    {
        var userNotFoundResult = NotFound(new ErrorResponseDto
        {
            ErrorType = EErrorType.EntityNotFound,
            Message = "User matching authenticated user not found",
        });
        
        var userId = HttpContext.User.GetUserIdIfExists();
        if (userId == null)
        {
            return userNotFoundResult;
        }

        var user = await _identityUow.UserService.GetUserWithRolesAsync(userId.Value);
        if (user == null)
        {
            return userNotFoundResult;
        }

        return new MeResponseDtoV1
        {
            User = new UserDtoV1
            {
                Id = user.Id,
                UserName = user.UserName!,
                NormalizedUserName = user.UserName!,
                IsApproved = user.IsApproved,
                Roles = user.UserRoles!.Select(ur => new RoleDtoV1
                {
                    Id = ur.Role!.Id,
                    Name = ur.Role!.Name!,
                    NormalizedName = ur.Role!.NormalizedName!,
                })
            }
        };
    }
}