using System.Linq.Expressions;
using Contracts.Domain;

namespace Contracts.DAL;

public interface IBaseEntityRepository<TDomainEntity, TEntity, TKey>
    where TDomainEntity : class, IIdDatabaseEntity<TKey>
    where TEntity : IIdDatabaseEntity<TKey>
    where TKey : struct, IEquatable<TKey>
{
    public Task<TEntity?> GetByIdAsync(TKey id);
    public Task<ICollection<TEntity>> GetAllAsync(params Expression<Func<TDomainEntity, bool>>[] filters);
    public TEntity Add(TEntity entity);
    public void Remove(TEntity entity);
    public Task RemoveAsync(TKey id);
    public void Update(TEntity entity);

    public TDomainEntity Map(TEntity entity);
    public TDomainEntity Map(TEntity entity, TDomainEntity domainEntity);

    public Task<bool> ExistsAsync(TKey id);

    public TDomainEntity? GetTrackedEntity(TEntity entity) => GetTrackedEntity(entity.Id);
    public TDomainEntity? GetTrackedEntity(TKey id);
}

public interface IBaseEntityRepository<TDomainEntity, TEntity> : IBaseEntityRepository<TDomainEntity, TEntity, Guid>
    where TDomainEntity : class, IIdDatabaseEntity<Guid>
    where TEntity : IIdDatabaseEntity<Guid>
{
}