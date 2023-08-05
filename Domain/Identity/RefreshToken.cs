using Base.Domain;

namespace Domain.Identity;

public class RefreshToken : BaseRefreshToken
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public RefreshToken() : this(7)
    {
    }

    public RefreshToken(double expiresInDays) : base(TimeSpan.FromDays(expiresInDays))
    {
    }
}