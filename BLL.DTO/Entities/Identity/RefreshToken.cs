using Base.Domain;

namespace BLL.DTO.Entities.Identity;

public class RefreshToken : BaseRefreshToken
{
    public RefreshToken(TimeSpan expiresIn) : base(expiresIn)
    {
    }

    public RefreshToken() : this(7)
    {
    }
    
    public RefreshToken(int expiresInDays) : base(TimeSpan.FromDays(expiresInDays))
    {
    }
}