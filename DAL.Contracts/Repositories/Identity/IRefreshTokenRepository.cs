using System.Linq.Expressions;
using Contracts.DAL;
using DAL.DTO.Entities.Identity;

namespace DAL.Contracts.Repositories.Identity;

public interface IRefreshTokenRepository : IBaseEntityRepository<Domain.Identity.RefreshToken, RefreshToken>
{
    public Task<ICollection<RefreshToken>> GetAllByUserIdAsync(Guid userId, params Expression<Func<Domain.Identity.RefreshToken, bool>>[] filters);
    public Task<ICollection<RefreshToken>> GetAllFullyExpiredByUserIdAsync(Guid userId);
    public Task<ICollection<RefreshToken>> GetAllValidByUserIdAndRefreshTokenAsync(
        Guid userId, string refreshToken);
    public Task ExecuteDeleteUserRefreshTokensAsync(Guid userId, string refreshToken);
}