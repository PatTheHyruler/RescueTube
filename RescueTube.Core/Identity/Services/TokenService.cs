using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using RescueTube.Core.Data.Extensions.Identity;
using RescueTube.Core.DTO.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RescueTube.Core.DTO.Entities.Identity;
using RescueTube.Core.Identity.Exceptions;
using RescueTube.Core.Identity.Options;

namespace RescueTube.Core.Identity.Services;

/// <summary>
/// For use later when adding API support. Requires review.
/// </summary>
public class TokenService
{
    private readonly IdentityUow _identityUow;
    private readonly JwtBearerOptions _jwtBearerOptions;

    public TokenService(IOptions<JwtBearerOptions> jwtBearerOptions, IdentityUow identityUow)
    {
        _identityUow = identityUow;
        _jwtBearerOptions = jwtBearerOptions.Value;
    }

    /// <summary>
    /// Create a new refresh token.
    /// </summary>
    /// <remarks>Requires calling SaveChanges().</remarks>
    /// <param name="userId">The ID of the user to create a refresh token for.</param>
    /// <param name="jwt">The JWT to create a refresh token for.</param>
    /// <returns>The created refresh token.</returns>
    public RefreshToken CreateAndAddRefreshToken(Guid userId, string jwt)
    {
        var token = new RefreshToken(
            TimeSpan.FromDays(_jwtBearerOptions.RefreshTokenExpiresInDays),
            HashJwt(jwt),
            userId);
        _identityUow.DbCtx.RefreshTokens.Add(token.ToDomainToken());
        return token;
    }

    private static string HashJwt(string jwt)
    {
        var textBytes = System.Text.Encoding.UTF8.GetBytes(jwt);
        var hashBytes = SHA512.HashData(textBytes);

        var hash = BitConverter
            .ToString(hashBytes)
            .Replace("-", string.Empty);

        return hash;
    }

    /// <summary>
    /// Generate a new JWT for given <see cref="ClaimsPrincipal"/>.
    /// </summary>
    /// <param name="claimsPrincipal">The <see cref="ClaimsPrincipal"/> to create a JWT for.</param>
    /// <param name="expiresInSeconds">The amount of seconds the JWT should be valid for</param>
    /// <param name="audienceSuffix">Suffix to append to the default configured JWT audience</param>
    /// <returns>The created JWT.</returns>
    public string GenerateJwt(ClaimsPrincipal claimsPrincipal, int? expiresInSeconds = null,
        string? audienceSuffix = null)
    {
        if (expiresInSeconds is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(expiresInSeconds), expiresInSeconds,
                "JWT expiration must be positive");
        }

        return IdentityHelpers.GenerateJwt(
            claimsPrincipal.Claims,
            _jwtBearerOptions.Key,
            _jwtBearerOptions.Issuer,
            _jwtBearerOptions.Audience + audienceSuffix,
            expiresInSeconds ?? _jwtBearerOptions.ExpiresInSeconds
        );
    }

    /// <summary>
    /// Generate a new JWT and updated refresh token, based on an old, potentially expired JWT
    /// and its corresponding (not expired) refresh token.
    /// </summary>
    /// <remarks>Requires calling SaveChanges().</remarks>
    /// <param name="jwt">The JWT to generate a replacement for.</param>
    /// <param name="refreshToken">
    /// The refresh token to refresh the JWT with.
    /// A new refresh token will be generated and the old refresh token will be invalidated.
    /// </param>
    /// <returns>The new JWT and new refresh token.</returns>
    /// <exception cref="InvalidJwtException">The provided JWT has an invalid form.</exception>
    /// <exception cref="InvalidRefreshTokenException">The provided refresh token is invalid.</exception>
    public async Task<JwtResult> RefreshTokenAsync(string jwt, string refreshToken)
    {
        JwtSecurityToken jwtToken;

        try
        {
            jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(jwt)
                       ?? throw new InvalidJwtException();
        }
        catch (ArgumentException)
        {
            throw new InvalidJwtException();
        }

        ValidateJwtGetPrincipal(jwt, ignoreExpiration: true);

        var userName = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value
                       ?? throw new InvalidJwtException();
        var user = await _identityUow.UserManager.FindByNameAsync(userName)
                   ?? throw new InvalidJwtException();

        var userRefreshTokens = await _identityUow.DbCtx.RefreshTokens
            .FilterValid(user.Id, refreshToken, HashJwt(jwt))
            .ToListAsync();
        if (userRefreshTokens.Count == 0)
        {
            throw new InvalidRefreshTokenException();
        }

        var claimsPrincipal = await _identityUow.SignInManager.CreateUserPrincipalAsync(user);
        jwt = GenerateJwt(claimsPrincipal);

        var userRefreshToken = userRefreshTokens.Single();
        userRefreshToken.ExpiresAt =
            DateTimeOffset.UtcNow.AddMinutes(
                _jwtBearerOptions.ExtendOldRefreshTokenExpirationByMinutes);

        var newRefreshToken = CreateAndAddRefreshToken(user.Id, jwt);

        return new JwtResult
        {
            Jwt = jwt,
            RefreshToken = newRefreshToken,
        };
    }

    /// <summary>
    /// Delete a refresh token.
    /// </summary>
    /// <remarks>
    /// Does not require calling SaveChanges().
    /// </remarks>
    /// <param name="jwt">
    /// The JWT that the provided refresh token belongs to.<br/>
    /// Required to know the user that the refresh token belongs to,
    /// and to authorize the refresh token's deletion.
    /// </param>
    /// <param name="refreshToken">The refresh token to be deleted.</param>
    /// <exception cref="InvalidJwtException">The provided JWT is invalid.</exception>
    public async Task DeleteRefreshTokenAsync(string jwt, string refreshToken)
    {
        ClaimsPrincipal principal;
        try
        {
            principal = IdentityHelpers.GetClaimsPrincipal(
                jwt, _jwtBearerOptions.Key, _jwtBearerOptions.Issuer, _jwtBearerOptions.Audience);
        }
        catch (Exception)
        {
            throw new InvalidJwtException();
        }

        ValidateJwtGetPrincipal(jwt, ignoreExpiration: true);

        var userId = principal.GetUserIdIfExists() ?? throw new InvalidJwtException();
        var jwtHash = HashJwt(jwt);
        await _identityUow.DbCtx.RefreshTokens
            .Filter(userId, refreshToken, jwtHash)
            .ExecuteDeleteAsync();
    }

    public async Task DeleteExpiredRefreshTokensAsync()
    {
        await _identityUow.DbCtx.RefreshTokens
            .Where(r => r.ExpiresAt < DateTimeOffset.UtcNow)
            .ExecuteDeleteAsync();
    }

    public DecodedJwt ValidateJwt(
        string jwt,
        bool ignoreExpiration = true,
        string? audienceSuffix = null)
    {
        var validationParameters = IdentityHelpers.GetValidationParameters(
            key: _jwtBearerOptions.Key, issuer: _jwtBearerOptions.Issuer,
            audience: _jwtBearerOptions.Audience + audienceSuffix,
            validateLifeTime: !ignoreExpiration
        );
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(jwt, validationParameters, out var securityToken);
            return new DecodedJwt(principal, securityToken);
        }
        catch (Exception)
        {
            throw new InvalidJwtException();
        }
    }

    public ClaimsPrincipal ValidateJwtGetPrincipal(string jwt, bool ignoreExpiration = true, string? audienceSuffix = null)
    {
        return ValidateJwt(jwt, ignoreExpiration: ignoreExpiration, audienceSuffix: audienceSuffix)
            .Principal;
    }
}