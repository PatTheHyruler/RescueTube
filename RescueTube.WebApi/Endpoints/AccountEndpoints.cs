using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RescueTube.Core.Identity;
using RescueTube.Core.Identity.Exceptions;
using RescueTube.Core.Identity.Services;
using RescueTube.Core.Utils;
using WebApp.ApiModels;
using WebApp.ApiModels.Auth;
using WebApp.Auth;

namespace WebApp.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var accountGroup = app.MapGroup("account").WithTags("Account");

        accountGroup.MapPost("register", RegisterAccountAsync)
            .AllowAnonymous()
            .HasApiVersion(1);

        accountGroup.MapPost("login", LoginAsync)
            .AllowAnonymous()
            .HasApiVersion(1);

        accountGroup.MapPost("RefreshToken", RefreshTokenAsync)
            .AllowAnonymous()
            .HasApiVersion(1);

        accountGroup.MapPost("logout", LogoutAsync)
            .AllowAnonymous()
            .HasApiVersion(1);

        accountGroup.MapGet("me", GetMeAsync)
            .RequireAuthorization()
            .HasApiVersion(1);

        accountGroup.MapPost("HangfireToken", CreateHangfireToken)
            .RequireAuthorization(p => p.RequireRole(RoleNames.AdminRoles))
            .HasApiVersion(1);
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="registrationData">Required data for registering a new user.</param>
    /// <param name="identityUow"></param>
    /// <param name="ct"></param>
    /// <returns>New JWT and refresh token (with expiration date) for the registered account, if registration doesn't require further approval.</returns>
    /// <response code="200">The registration was successful.</response>
    /// <response code="400">User with provided username is already registered or provided registration data was invalid.</response>
    private static async Task<Results<Ok<JwtResponseDtoV1>, BadRequest<ErrorResponseDto>>> RegisterAccountAsync(
        [FromBody] RegisterDtoV1 registrationData, [FromServices] IdentityUow identityUow, CancellationToken ct)
    {
        try
        {
            using (var transaction = TransactionUtils.NewTransactionScope())
            {
                await identityUow.UserService.RegisterUserAsync(
                    username: registrationData.Username,
                    password: registrationData.Password);
                await identityUow.SaveChangesAsync(ct);
                transaction.Complete();
            }

            using (var transaction = TransactionUtils.NewTransactionScope())
            {
                var jwtResult =
                    await identityUow.UserService.SignInJwtAsync(registrationData.Username, registrationData.Password);
                await identityUow.SaveChangesAsync(ct);
                transaction.Complete();

                return TypedResults.Ok(new JwtResponseDtoV1
                {
                    Jwt = jwtResult.Jwt,
                    RefreshToken = jwtResult.RefreshToken.Token,
                    RefreshTokenExpiresAt = jwtResult.RefreshToken.ExpiresAt,
                });
            }
        }
        catch (IdentityOperationFailedException e)
        {
            return TypedResults.BadRequest(new ErrorResponseDto
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
            return TypedResults.BadRequest(new ErrorResponseDto
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
    /// <param name="identityUow"></param>
    /// <param name="ct"></param>
    /// <returns>New JWT and refresh token (with expiration date), if login was successful.</returns>
    /// <response code="200">Login was successful.</response>
    /// <response code="400">Username or password was invalid, or user account hasn't been approved yet.</response>
    private static async Task<Results<Ok<JwtResponseDtoV1>, BadRequest<ErrorResponseDto>>> LoginAsync(
        [FromBody] LoginDtoV1 loginData, [FromServices] IdentityUow identityUow, CancellationToken ct)
    {
        try
        {
            using var transaction = TransactionUtils.NewTransactionScope();
            var jwtResult = await identityUow.UserService.SignInJwtAsync(loginData.Username, loginData.Password);
            await identityUow.SaveChangesAsync(ct);
            transaction.Complete();

            return TypedResults.Ok(new JwtResponseDtoV1
            {
                Jwt = jwtResult.Jwt,
                RefreshToken = jwtResult.RefreshToken.Token,
                RefreshTokenExpiresAt = jwtResult.RefreshToken.ExpiresAt,
            });
        }
        catch (UserNotFoundException)
        {
            return TypedResults.BadRequest(new ErrorResponseDto
            {
                ErrorType = EErrorType.InvalidLoginCredentials,
            });
        }
        catch (WrongPasswordException)
        {
            return TypedResults.BadRequest(new ErrorResponseDto
            {
                ErrorType = EErrorType.InvalidLoginCredentials,
            });
        }
        catch (UserNotApprovedException)
        {
            return TypedResults.BadRequest(new ErrorResponseDto
            {
                ErrorType = EErrorType.UserNotApproved,
            });
        }
    }

    /// <summary>
    /// Get a new JWT and refresh token, using existing JWT and refresh token.
    /// </summary>
    /// <param name="refreshTokenModel">Tokens to refresh</param>
    /// <param name="identityUow"></param>
    /// <param name="ct"></param>
    /// <returns>Refreshed JWT and refresh token (with expiration date), if refreshing was successful.</returns>
    /// <response code="200">Token refresh was successful.</response>
    /// <response code="400">Provided token/tokens was/were invalid.</response>
    private static async Task<Results<Ok<JwtResponseDtoV1>, BadRequest<ErrorResponseDto>>> RefreshTokenAsync(
        [FromBody] RefreshTokenRequestDtoV1 refreshTokenModel,
        [FromServices] IdentityUow identityUow, CancellationToken ct)
    {
        try
        {
            using var transaction = TransactionUtils.NewTransactionScope();
            var jwtResult = await identityUow.TokenService.RefreshTokenAsync(
                jwt: refreshTokenModel.Jwt,
                refreshToken: refreshTokenModel.RefreshToken);
            await identityUow.SaveChangesAsync(ct);
            transaction.Complete();

            return TypedResults.Ok(new JwtResponseDtoV1
            {
                Jwt = jwtResult.Jwt,
                RefreshToken = jwtResult.RefreshToken.Token,
                RefreshTokenExpiresAt = jwtResult.RefreshToken.ExpiresAt,
            });
        }
        catch (InvalidJwtException)
        {
            return TypedResults.BadRequest(new ErrorResponseDto
            {
                ErrorType = EErrorType.InvalidJwt,
            });
        }
        catch (InvalidRefreshTokenException)
        {
            return TypedResults.BadRequest(new ErrorResponseDto
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
    /// <param name="logoutDto">The refresh token to delete.</param>
    /// <param name="identityUow"></param>
    /// <param name="ct"></param>
    /// <response code="200">Refresh token deleted successfully.</response>
    /// <response code="400">Invalid JWT provided.</response>
    private static async Task<Results<Ok, BadRequest<ErrorResponseDto>>> LogoutAsync(
        [FromBody] LogoutDtoV1 logoutDto,
        [FromServices] IdentityUow identityUow, CancellationToken ct)
    {
        try
        {
            await identityUow.TokenService.DeleteRefreshTokenAsync(
                jwt: logoutDto.Jwt, refreshToken: logoutDto.RefreshToken, ct);
            await identityUow.SaveChangesAsync(ct);
            return TypedResults.Ok();
        }
        catch (InvalidJwtException)
        {
            return TypedResults.BadRequest(new ErrorResponseDto
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
    private static async Task<Results<Ok<MeResponseDtoV1>, NotFound<ErrorResponseDto>>> GetMeAsync(
        [FromServices] IdentityUow identityUow, HttpContext httpContext, CancellationToken ct)
    {
        var userNotFoundResult = TypedResults.NotFound(new ErrorResponseDto
        {
            ErrorType = EErrorType.EntityNotFound,
            Message = "User matching authenticated user not found",
        });

        var userId = httpContext.User.GetUserIdIfExists();
        if (userId == null)
        {
            return userNotFoundResult;
        }

        var user = await identityUow.UserService.GetUserWithRolesAsync(userId.Value, ct);
        if (user == null)
        {
            return userNotFoundResult;
        }

        return TypedResults.Ok(new MeResponseDtoV1
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
                }),
            },
        });
    }

    /// <summary>
    /// Get a JWT token that can be passed to the Hangfire dashboard to authenticate.
    /// </summary>
    /// <returns>The JWT string.</returns>
    private static Ok<string> CreateHangfireToken([FromServices] IdentityUow identityUow, ClaimsPrincipal user)
    {
        return TypedResults.Ok(
            identityUow.TokenService.GenerateJwt(user, expiresInSeconds: 60, RescueTubeIdentity.HangfireJwtSuffix));
    }
}