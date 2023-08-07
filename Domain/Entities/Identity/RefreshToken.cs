using Base.Domain;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities.Identity;

[Index(nameof(UserId), nameof(Token), nameof(JwtHash), nameof(ExpiresAt))]
public class RefreshToken : AbstractIdDatabaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Token { get; set; } = Guid.NewGuid().ToString();

    public DateTime ExpiresAt { get; set; }

    public string JwtHash { get; set; } = default!;

    public bool IsExpired => ExpiresAt <= DateTime.UtcNow;
}