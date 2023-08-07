using AutoMapper;
using AutoMapper.QueryableExtensions;
using DAL.Contracts;
using DAL.Contracts.Repositories.Identity;
using DAL.DTO.Entities.Identity;
using DAL.EF.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DAL.EF.Repositories.Identity;

public class RefreshTokenRepository : BaseAppEntityRepository<Domain.Entities.Identity.RefreshToken, RefreshToken>,
    IRefreshTokenRepository
{
    public RefreshTokenRepository(AbstractAppDbContext dbContext, IMapper mapper, IAppUnitOfWork uow) : base(dbContext,
        mapper, uow)
    {
    }

    public async Task<ICollection<RefreshToken>> GetAllValidAsync(Guid userId, string refreshToken, string jwtHash)
    {
        return await Entities
            .Where(e => e.UserId == userId && e.Token == refreshToken &&
                        e.JwtHash == jwtHash && e.ExpiresAt > DateTime.UtcNow)
            .ProjectTo<RefreshToken>(Mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public Task ExecuteDeleteUserRefreshTokensAsync(Guid userId, string refreshToken, string jwtHash)
    {
        return Entities
            .Where(r => r.UserId == userId &&
                        r.Token == refreshToken && r.JwtHash == jwtHash)
            .ExecuteDeleteAsync();
    }

    public override void Update(RefreshToken entity)
    {
        Update(entity,
            e => e.UserId,
            e => e.Token,
            e => e.JwtHash,
            e => e.ExpiresAt
        );
    }
}