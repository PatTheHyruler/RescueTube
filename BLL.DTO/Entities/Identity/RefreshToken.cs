using Base.Domain;

namespace BLL.DTO.Entities.Identity;

public class RefreshToken : AbstractIdDatabaseEntity
{
    public Guid UserId { get; set; }

    public string Token { get; set; } = Guid.NewGuid().ToString();

    public DateTime ExpiresAt { get; set; }

    public string JwtHash { get; set; } = default!;

    public bool IsExpired => ExpiresAt <= DateTime.UtcNow;

    public RefreshToken()
    {
    }

    public RefreshToken(TimeSpan expiresIn, string jwtHash, Guid userId)
    {
        ExpiresAt = DateTime.UtcNow.AddMilliseconds(expiresIn.TotalMilliseconds);
        JwtHash = jwtHash;
        UserId = userId;
    }
}