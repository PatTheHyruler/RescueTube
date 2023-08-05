using System.ComponentModel.DataAnnotations;
using Contracts.Domain;

namespace Base.Domain;

public class BaseRefreshToken : BaseRefreshToken<Guid>, IIdDatabaseEntity
{
    public BaseRefreshToken(TimeSpan expiresIn) : base(expiresIn)
    {
    }
}

public class BaseRefreshToken<TKey> : AbstractIdDatabaseEntity<TKey> where TKey : struct, IEquatable<TKey>
{
    [MaxLength(64)] public string RefreshToken { get; set; } = Guid.NewGuid().ToString();

    public DateTime ExpiresAt { get; set; }

    [MaxLength(64)] public string? PreviousRefreshToken { get; set; }
    public DateTime? PreviousExpiresAt { get; set; }

    public void Refresh(TimeSpan extendOldExpirationBy, TimeSpan expiresInDays)
    {
        PreviousRefreshToken = RefreshToken;
        PreviousExpiresAt = DateTime.UtcNow.AddMilliseconds(extendOldExpirationBy.TotalMilliseconds);

        RefreshToken = Guid.NewGuid().ToString();
        ExpiresAt = DateTime.UtcNow.AddMilliseconds(expiresInDays.TotalMilliseconds);
    }

    public BaseRefreshToken(TimeSpan expiresIn)
    {
        ExpiresAt = DateTime.UtcNow.AddMilliseconds(expiresIn.TotalMilliseconds);
    }

    public bool IsExpired => ExpiresAt <= DateTime.UtcNow;

    public bool IsFullyExpired => IsExpired && PreviousExpiresAt != null && PreviousExpiresAt <= DateTime.UtcNow;
}