using Base.Domain;

namespace DAL.DTO.Entities.Identity;

public class RefreshToken : BaseRefreshToken
{
    public Guid UserId { get; set; } = default!;

    public RefreshToken() : this(TimeSpan.FromDays(7))
    {
    }

    public RefreshToken(TimeSpan expiresIn) : base(expiresIn)
    {
    }
}