using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BLL.DTO.Entities.Identity;
using BLL.DTO.Exceptions.Identity;
using BLL.DTO.Mappers;
using BLL.Identity.Options;
using DAL.EF.Extensions.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BLL.Identity.Services;

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
        _identityUow.Ctx.RefreshTokens.Add(token.ToDomainToken());
        return token;
    }

    private static string HashJwt(string jwt)
    {
        using var sha = new System.Security.Cryptography.HMACSHA256();
        var textBytes = System.Text.Encoding.UTF8.GetBytes(jwt);
        var hashBytes = sha.ComputeHash(textBytes);

        var hash = BitConverter
            .ToString(hashBytes)
            .Replace("-", string.Empty);

        return hash;
    }

    /// <summary>
    /// Validate the requested expiration time for a JWT.
    /// </summary>
    /// <param name="expiresInSeconds">The requested JWT expiration time, in seconds.</param>
    /// <returns>The expiration time, if provided, else returns the default configured expiration time.</returns>
    /// <exception cref="InvalidJwtExpirationRequestedException">Thrown if the requested expiration time is too short or too long.</exception>
    private int ValidateExpiresInSeconds(int? expiresInSeconds)
    {
        if (expiresInSeconds == null) return _jwtBearerOptions.ExpiresInSeconds;
        if (expiresInSeconds <= 1 || expiresInSeconds > _jwtBearerOptions.ExpiresInSecondsMax)
            throw new InvalidJwtExpirationRequestedException(expiresInSeconds.Value, 1,
                _jwtBearerOptions.ExpiresInSecondsMax);

        return expiresInSeconds.Value;
    }

    /// <summary>
    /// Generate a new JWT for given <see cref="ClaimsPrincipal"/>.
    /// </summary>
    /// <param name="claimsPrincipal">The <see cref="ClaimsPrincipal"/> to create a JWT for.</param>
    /// <param name="expiresInSeconds">The amount of time (in seconds) that the created JWT should be valid for.</param>
    /// <returns>The created JWT.</returns>
    /// <exception cref="InvalidJwtExpirationRequestedException">Thrown if the requested expiration time is too short or too long.</exception>
    public string GenerateJwt(ClaimsPrincipal claimsPrincipal, int? expiresInSeconds)
    {
        return IdentityHelpers.GenerateJwt(
            claimsPrincipal.Claims,
            _jwtBearerOptions.Key,
            _jwtBearerOptions.Issuer,
            _jwtBearerOptions.Audience,
            ValidateExpiresInSeconds(expiresInSeconds)
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
    /// <param name="expiresInSeconds">The amount of time the new JWT should be valid for.</param>
    /// <returns>The new JWT and new refresh token.</returns>
    /// <exception cref="InvalidJwtException">The provided JWT has an invalid form.</exception>
    /// <exception cref="InvalidRefreshTokenException">The provided refresh token is invalid.</exception>
    /// <exception cref="InvalidJwtExpirationRequestedException">The requested expiration time is too short or too long.</exception>
    public async Task<JwtResult> RefreshTokenAsync(string jwt, string refreshToken, int? expiresInSeconds = null)
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

        if (!IdentityHelpers.ValidateToken(jwt, _jwtBearerOptions.Key, _jwtBearerOptions.Issuer,
                _jwtBearerOptions.Audience, ignoreExpiration: true))
        {
            throw new InvalidJwtException();
        }

        var userName = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value
                       ?? throw new InvalidJwtException();
        var user = await _identityUow.UserManager.FindByNameAsync(userName)
                   ?? throw new InvalidJwtException();

        var userRefreshTokens = await _identityUow.Ctx.RefreshTokens
            .FilterValid(user.Id, refreshToken, HashJwt(jwt))
            .ToListAsync();
        if (userRefreshTokens.Count == 0)
        {
            throw new InvalidRefreshTokenException();
        }

        var claimsPrincipal = await _identityUow.SignInManager.CreateUserPrincipalAsync(user);
        jwt = GenerateJwt(claimsPrincipal, expiresInSeconds);

        var userRefreshToken = userRefreshTokens.Single();
        userRefreshToken.ExpiresAt =
            userRefreshToken.ExpiresAt.AddMinutes(
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

        if (!IdentityHelpers.ValidateToken(jwt, _jwtBearerOptions.Key,
                _jwtBearerOptions.Issuer, _jwtBearerOptions.Audience,
                ignoreExpiration: true))
        {
            throw new InvalidJwtException();
        }

        var userId = principal.GetUserIdIfExists() ?? throw new InvalidJwtException();
        var jwtHash = HashJwt(jwt);
        await _identityUow.Ctx.RefreshTokens
            .Filter(userId, refreshToken, jwtHash)
            .ExecuteDeleteAsync();
    }
}