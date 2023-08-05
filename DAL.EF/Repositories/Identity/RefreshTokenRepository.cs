using System.Linq.Expressions;
using AutoMapper;
using DAL.Contracts;
using DAL.Contracts.Repositories.Identity;
using DAL.DTO.Entities.Identity;
using DAL.EF.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DAL.EF.Repositories.Identity;

public class RefreshTokenRepository : BaseAppEntityRepository<Domain.Identity.RefreshToken, RefreshToken>,
    IRefreshTokenRepository
{
    public RefreshTokenRepository(AbstractAppDbContext dbContext, IMapper mapper, IAppUnitOfWork uow) : base(dbContext,
        mapper, uow)
    {
    }

    protected override Domain.Identity.RefreshToken AfterMap(RefreshToken entity, Domain.Identity.RefreshToken mapped)
    {
        mapped.User = Uow.Users.GetTrackedEntity(entity.UserId);

        return mapped;
    }

    public async Task<ICollection<RefreshToken>> GetAllByUserIdAsync(Guid userId,
        params Expression<Func<Domain.Identity.RefreshToken, bool>>[] filters)
    {
        var newFilters = new List<Expression<Func<Domain.Identity.RefreshToken, bool>>>
        {
            rt => rt.UserId == userId
        };
        newFilters.AddRange(filters);
        return await GetAllAsync(newFilters.ToArray());
    }

    public async Task<ICollection<RefreshToken>> GetAllFullyExpiredByUserIdAsync(Guid userId)
    {
        return (await GetAllByUserIdAsync(userId)).Where(r => r.IsFullyExpired).ToList();
    }

    public async Task<ICollection<RefreshToken>> GetAllValidByUserIdAndRefreshTokenAsync(Guid userId,
        string refreshToken)
    {
        return await GetAllByUserIdAsync(userId, r =>
            (r.RefreshToken == refreshToken && r.ExpiresAt > DateTime.UtcNow) ||
            (r.PreviousRefreshToken == refreshToken && r.PreviousExpiresAt > DateTime.UtcNow));
    }

    public async Task ExecuteDeleteUserRefreshTokensAsync(Guid userId, string refreshToken)
    {
        await Entities
            .Where(r => r.UserId == userId &&
                r.RefreshToken == refreshToken || r.PreviousRefreshToken == refreshToken)
            .ExecuteDeleteAsync();
    }
}