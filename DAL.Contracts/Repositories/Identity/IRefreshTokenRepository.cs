using Contracts.DAL;
using DAL.DTO.Entities.Identity;

namespace DAL.Contracts.Repositories.Identity;

public interface IRefreshTokenRepository : IBaseEntityRepository<Domain.Entities.Identity.RefreshToken, RefreshToken>
{
    public Task<ICollection<RefreshToken>> GetAllValidAsync(
        Guid userId, string refreshToken, string jwtHash);
    public Task ExecuteDeleteUserRefreshTokensAsync(Guid userId, string refreshToken, string jwtHash);
}