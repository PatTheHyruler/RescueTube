using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts.DAL;
using Contracts.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Base.DAL.EF;

/// <summary>
/// Base class for repository classes that serve as an abstraction between
/// Entity Framework Core's DbSet/DbContext and the rest of the application.
/// </summary>
/// <remarks>
/// Regarding updates:<br/><br/>
/// 
/// Because EF Core doesn't track entities remapped to DTOs using ProjectTo,
/// the entry will get modified and redundant data will be submitted.<br/>
/// Presumably only querying required data using ProjectTo is still worth it,
/// since likely most queries will be reads?<br/>
/// TODO: Test this assumption maybe?
/// <br/><br/>
/// 
/// This behaviour also avoids uncertainty regarding whether
/// fetch or update should take priority.
/// Fetched data is not tracked by the change tracker, only added/updated/removed data is.<br/>
/// This also means that in a situation where an entity is fetched and updated (but not yet committed),
/// performing another fetch will return the non-updated values from the database, not the modified in-memory state.
/// Whether this behaviour is desirable is debatable.<br/>
/// TODO: Figure out whether submitting some unnecessary data sometimes is more performant than manually re-adding fetched data to the change tracker.
/// </remarks>
public abstract class BaseEntityRepository<TDomainEntity, TDtoEntity, TKey, TDbContext, TUow> :
    IBaseEntityRepository<TDomainEntity, TDtoEntity, TKey>
    where TDomainEntity : class, IIdDatabaseEntity<TKey>, new()
    where TKey : struct, IEquatable<TKey>
    where TDbContext : DbContext
    where TDtoEntity : class, IIdDatabaseEntity<TKey>
    where TUow : IBaseUnitOfWork
{
    public TDbContext DbContext { get; }
    public readonly IMapper Mapper;

    public TUow Uow { get; }

    public BaseEntityRepository(TDbContext dbContext, IMapper mapper, TUow uow)
    {
        DbContext = dbContext;
        Mapper = mapper;
        Uow = uow;
    }

    protected DbSet<TDomainEntity> Entities =>
        DbContext
            .GetType()
            .GetProperties()
            .FirstOrDefault(pi => pi.PropertyType == typeof(DbSet<TDomainEntity>))
            ?.GetValue(DbContext) as DbSet<TDomainEntity> ??
        throw new ApplicationException(
            $"Failed to fetch DbSet for Entity type {typeof(TDomainEntity)} from {typeof(DbContext)}");

    protected virtual TQueryable IncludeDefaults<TQueryable>(TQueryable queryable)
        where TQueryable : IQueryable<TDomainEntity>
    {
        return queryable;
    }

    protected IQueryable<TDomainEntity> EntitiesWithDefaults => IncludeDefaults(Entities);

    public async Task<TDtoEntity?> GetByIdAsync(TKey id)
    {
        return await EntitiesWithDefaults.ProjectTo<TDtoEntity>(Mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(e => e.Id.Equals(id));
    }

    protected IQueryable<TDomainEntity> ApplyFilters(IQueryable<TDomainEntity> query,
        params Expression<Func<TDomainEntity, bool>>[] filters)
    {
        foreach (var filter in filters)
        {
            query = query.Where(filter);
        }

        return query;
    }

    protected IQueryable<TDomainEntity> ApplyFilters(params Expression<Func<TDomainEntity, bool>>[] filters)
    {
        return ApplyFilters(Entities, filters);
    }

    public Task<ICollection<TDtoEntity>> GetAllAsync(params Expression<Func<TDomainEntity, bool>>[] filters)
    {
        return GetAllAsync(default, filters);
    }

    public async Task<ICollection<TDtoEntity>> GetAllAsync(CancellationToken ct = default, params Expression<Func<TDomainEntity, bool>>[] filters)
    {
        var result = IncludeDefaults(ApplyFilters(filters));
        return await Mapper.ProjectTo<TDtoEntity>(result).ToListAsync(cancellationToken: ct);
    }

    public TDtoEntity Add(TDtoEntity entity)
    {
        Entities.Add(Mapper.Map<TDomainEntity>(entity));
        return entity;
    }

    public void Remove(TKey id)
    {
        Remove(GetUpdatableEntry(id).Entity);
    }

    public void Remove(TDtoEntity entity)
    {
        Remove(entity.Id);
    }

    private void Remove(TDomainEntity entity)
    {
        Entities.Remove(entity);
    }

    public async Task ExecuteDeleteAsync(TKey id, CancellationToken ct = default)
    {
        await Entities.Where(e => e.Id.Equals(id)).ExecuteDeleteAsync(ct);
    }

    public async Task RemoveAsync(TKey id)
    {
        Remove(await Entities.FindAsync(id) ??
               throw new ApplicationException($"Failed to delete entity with ID {id} - entity not found!"));
    }

    public abstract void Update(TDtoEntity entity);

    protected void Update<TCustomDtoEntity>(TCustomDtoEntity entity, params Expression<Func<TDomainEntity, object?>>[] changedPropertyExpressions)
        where TCustomDtoEntity : IIdDatabaseEntity<TKey>
    {
        Update(Mapper.Map<TDomainEntity>(entity), changedPropertyExpressions);
    }

    protected void Update(TDomainEntity values,
        params Expression<Func<TDomainEntity, object?>>[] changedPropertyExpressions)
    {
        var entry = GetUpdatableEntry(values.Id);
        entry.Update(values, changedPropertyExpressions);
    }

    public async Task<bool> ExistsAsync(TKey id, CancellationToken ct = default)
    {
        return await Entities.AnyAsync(e => e.Id.Equals(id), cancellationToken: ct);
    }

    private EntityEntry<TDomainEntity> GetUpdatableEntry(TKey id)
    {
        return DbContext.GetUpdatableEntry<TDomainEntity, TKey>(id);
    }

    public Task SaveChangesAsync()
    {
        return DbContext.SaveChangesAsync();
    }
}

public abstract class BaseEntityRepository<TDomainEntity, TDtoEntity, TDbContext, TUow> :
    BaseEntityRepository<TDomainEntity, TDtoEntity, Guid, TDbContext, TUow>,
    IBaseEntityRepository<TDomainEntity, TDtoEntity>
    where TDtoEntity : class, IIdDatabaseEntity<Guid>
    where TDbContext : DbContext
    where TDomainEntity : class, IIdDatabaseEntity<Guid>, new()
    where TUow : IBaseUnitOfWork
{
    public BaseEntityRepository(TDbContext dbContext, IMapper mapper, TUow uow) : base(dbContext, mapper, uow)
    {
    }
}