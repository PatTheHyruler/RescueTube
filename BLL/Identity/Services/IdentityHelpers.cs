using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Identity.Services;

public static class IdentityHelpers
{
    public static string GenerateJwt(IEnumerable<Claim> claims, string key,
        string issuer, string audience,
        int expiresInSeconds)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddSeconds(expiresInSeconds);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: signingCredentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static bool ValidateToken(string jwt, string key,
        string issuer, string audience, bool ignoreExpiration = true)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = GetValidationParameters(key, issuer, audience, !ignoreExpiration);

        try
        {
            tokenHandler.ValidateToken(jwt, validationParameters, out _);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    private static TokenValidationParameters GetValidationParameters(string key,
        string issuer, string audience, bool validateLifeTime = true)
    {
        return new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidIssuer = issuer,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = validateLifeTime,
        };
    }

    public static ClaimsPrincipal GetClaimsPrincipal(string jwt, string key, string issuer, string audience,
        bool ignoreExpiration = true)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = GetValidationParameters(key, issuer, audience, !ignoreExpiration);

        return tokenHandler.ValidateToken(jwt, validationParameters, out _);
    }

    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        return Guid.Parse(
            user.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
    }

    public static Guid? GetUserIdIfExists(this ClaimsPrincipal? user)
    {
        var stringId = user?.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return stringId == null ? null : Guid.Parse(stringId);
    }

    public static bool IsAdmin(this ClaimsPrincipal? user)
    {
        if (user == null)
        {
            return false;
        }
        return RoleNames.AdminRoles.Any(user.IsInRole);
    }
}