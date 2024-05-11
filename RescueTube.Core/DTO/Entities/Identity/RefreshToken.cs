using System.Diagnostics.CodeAnalysis;
using RescueTube.Domain.Base;

namespace RescueTube.Core.DTO.Entities.Identity;

public class RefreshToken : BaseIdDbEntity
{
    public Guid UserId { get; set; }

    public string Token { get; set; } = Guid.NewGuid().ToString();

    public DateTime ExpiresAt { get; set; }

    public required string JwtHash { get; set; }

    public bool IsExpired => ExpiresAt <= DateTime.UtcNow;

    public RefreshToken()
    {
    }

    [SetsRequiredMembers]
    public RefreshToken(TimeSpan expiresIn, string jwtHash, Guid userId)
    {
        ExpiresAt = DateTime.UtcNow.AddMilliseconds(expiresIn.TotalMilliseconds);
        JwtHash = jwtHash;
        UserId = userId;
    }
}