using RescueTube.Domain.Entities.Identity;

namespace RescueTube.Core.Data.Extensions.Identity;

public static class RefreshTokenExtensions
{
    public static IQueryable<RefreshToken> FilterValid(this IQueryable<RefreshToken> entities,
        Guid userId, string refreshToken, string jwtHash)
    {
        return entities.Filter(userId, refreshToken, jwtHash)
            .Where(e => e.ExpiresAt > DateTimeOffset.UtcNow);
    }

    public static IQueryable<RefreshToken> Filter(this IQueryable<RefreshToken> entities,
        Guid userId, string refreshToken, string jwtHash)
    {
        return entities.Where(e =>
            e.UserId == userId &&
            e.Token == refreshToken && e.JwtHash == jwtHash);
    }
}