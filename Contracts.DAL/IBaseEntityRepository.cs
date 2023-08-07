using System.Linq.Expressions;
using Contracts.Domain;

namespace Contracts.DAL;

public interface IBaseEntityRepository<TDomainEntity, TDtoEntity, TKey>
    where TDomainEntity : class, IIdDatabaseEntity<TKey>
    where TDtoEntity : IIdDatabaseEntity<TKey>
    where TKey : struct, IEquatable<TKey>
{
    public Task<TDtoEntity?> GetByIdAsync(TKey id);
    public Task<ICollection<TDtoEntity>> GetAllAsync(params Expression<Func<TDomainEntity, bool>>[] filters);

    public Task<ICollection<TDtoEntity>> GetAllAsync(CancellationToken ct = default,
        params Expression<Func<TDomainEntity, bool>>[] filters);

    public TDtoEntity Add(TDtoEntity entity);
    public void Remove(TKey id);
    public void Remove(TDtoEntity entity);
    public Task ExecuteDeleteAsync(TKey id, CancellationToken ct = default);
    public Task RemoveAsync(TKey id);
    public void Update(TDtoEntity entity);

    public Task<bool> ExistsAsync(TKey id, CancellationToken ct = default);

    public Task SaveChangesAsync();
}

public interface
    IBaseEntityRepository<TDomainEntity, TDtoEntity> : IBaseEntityRepository<TDomainEntity, TDtoEntity, Guid>
    where TDomainEntity : class, IIdDatabaseEntity<Guid>
    where TDtoEntity : IIdDatabaseEntity<Guid>
{
}