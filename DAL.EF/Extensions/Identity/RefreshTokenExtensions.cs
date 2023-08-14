using Domain.Entities.Identity;

namespace DAL.EF.Extensions.Identity;

public static class RefreshTokenExtensions
{
    public static IQueryable<RefreshToken> FilterValid(this IQueryable<RefreshToken> entities,
        Guid userId, string refreshToken, string jwtHash)
    {
        return entities.Filter(userId, refreshToken, jwtHash)
            .Where(e => e.ExpiresAt > DateTime.UtcNow);
    }

    public static IQueryable<RefreshToken> Filter(this IQueryable<RefreshToken> entities,
        Guid userId, string refreshToken, string jwtHash)
    {
        return entities.Where(e =>
            e.UserId == userId &&
            e.Token == refreshToken && e.JwtHash == jwtHash);
    }
}