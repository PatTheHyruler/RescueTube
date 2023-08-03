using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts.DAL;
using Contracts.Domain;
using Microsoft.EntityFrameworkCore;

namespace Base.DAL.EF;

public class BaseEntityRepository<TDomainEntity, TEntity, TKey, TDbContext, TUow> :
    IBaseEntityRepository<TDomainEntity, TEntity, TKey>
    where TDomainEntity : class, IIdDatabaseEntity<TKey>
    where TKey : struct, IEquatable<TKey>
    where TDbContext : DbContext
    where TEntity : class, IIdDatabaseEntity<TKey>
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

    protected virtual TDomainEntity AfterMap(TEntity entity, TDomainEntity mapped)
    {
        return mapped;
    }

    protected virtual TQueryable IncludeDefaults<TQueryable>(TQueryable queryable)
        where TQueryable : IQueryable<TDomainEntity>
    {
        return queryable;
    }

    protected IQueryable<TDomainEntity> EntitiesWithDefaults => IncludeDefaults(Entities);

    public TDomainEntity Map(TEntity entity)
    {
        return AfterMap(entity, Mapper.Map<TEntity, TDomainEntity>(entity)!);
    }

    public TDomainEntity Map(TEntity entity, TDomainEntity domainEntity)
    {
        return AfterMap(entity, Mapper.Map(entity, domainEntity));
    }

    public async Task<TEntity?> GetByIdAsync(TKey id)
    {
        return AttachIfNotAttached(await EntitiesWithDefaults.ProjectTo<TEntity>(Mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(e => e.Id.Equals(id)));
    }

    protected IQueryable<TDomainEntity> GetAll(params Expression<Func<TDomainEntity, bool>>[] filters)
    {
        IQueryable<TDomainEntity> result = Entities;

        foreach (var filter in filters)
        {
            result = result.Where(filter);
        }

        return result;
    }

    public async Task<ICollection<TEntity>> GetAllAsync(params Expression<Func<TDomainEntity, bool>>[] filters)
    {
        var result = IncludeDefaults(GetAll(filters));
        return AttachIfNotAttached<ICollection<TEntity>, TEntity>(
            await Mapper.ProjectTo<TEntity>(result).ToListAsync());
    }

    protected TAddEntity AddBase<TAddEntity>(TAddEntity entity,
        Func<TAddEntity, TDomainEntity, TDomainEntity> mapTo, Func<TAddEntity, TDomainEntity> map)
        where TAddEntity : IIdDatabaseEntity<TKey>
    {
        var trackedEntity = GetTrackedEntity(entity.Id);
        if (trackedEntity != null)
        {
            if (DbContext.ChangeTracker.AutoDetectChangesEnabled)
            {
                mapTo(entity, trackedEntity);
            }
            else
            {
                Entities.Update(mapTo(entity, trackedEntity));
            }
        }
        else
        {
            Entities.Add(map(entity));
        }

        return entity;
    }

    public TEntity Add(TEntity entity)
    {
        return AddBase(entity, Map, Map);
    }

    public void Remove(TEntity entity)
    {
        Remove(GetTrackedEntity(entity) ??
               Mapper.Map<TEntity, TDomainEntity>(entity)!); // TODO: Should this throw if entity is not tracked?
    }

    private void Remove(TDomainEntity entity)
    {
        Entities.Remove(entity);
    }

    public async Task RemoveAsync(TKey id)
    {
        Remove(await Entities.FindAsync(id) ??
               throw new ApplicationException($"Failed to delete entity with ID {id} - entity not found!"));
    }

    protected void UpdateBase<TUpdateEntity>(TUpdateEntity entity,
        Func<TUpdateEntity, TDomainEntity, TDomainEntity> mapTo, Func<TUpdateEntity, TDomainEntity> map)
        where TUpdateEntity : IIdDatabaseEntity<TKey>
    {
        var trackedEntity = GetTrackedEntity(entity.Id);
        if (trackedEntity != null)
        {
            mapTo(entity, trackedEntity);
        }
        else
        {
            Entities.Update(map(entity));
        }
    }

    public virtual void Update(TEntity entity)
    {
        UpdateBase(entity, Map, Map);
    }

    public async Task<bool> ExistsAsync(TKey id)
    {
        return await Entities.AnyAsync(e => e.Id.Equals(id));
    }

    public TDomainEntity? GetTrackedEntity(TEntity entity) =>
        GetTrackedEntity(entity.Id);

    public TDomainEntity? GetTrackedEntity(TKey id) => DbContext.GetTrackedEntity<TDomainEntity, TKey>(id);

    protected TCustomEntityCollection AttachIfNotAttached<TCustomEntityCollection, TCustomEntity>(
        TCustomEntityCollection entities)
        where TCustomEntityCollection : ICollection<TCustomEntity>
        where TCustomEntity : class, IIdDatabaseEntity<TKey>
    {
        return entities.AttachIfNotAttached<TCustomEntityCollection, TCustomEntity, TDomainEntity, TKey>(Mapper,
            DbContext);
    }

    protected TCustomEntity? AttachIfNotAttached<TCustomEntity>(TCustomEntity? entity)
        where TCustomEntity : class, IIdDatabaseEntity<TKey>
    {
        return entity.AttachIfNotAttached<TCustomEntity, TDomainEntity, TKey>(Mapper, DbContext);
    }
}

public class BaseEntityRepository<TDomainEntity, TEntity, TDbContext, TUow> :
    BaseEntityRepository<TDomainEntity, TEntity, Guid, TDbContext, TUow>,
    IBaseEntityRepository<TDomainEntity, TEntity>
    where TEntity : class, IIdDatabaseEntity<Guid>
    where TDbContext : DbContext
    where TDomainEntity : class, IIdDatabaseEntity<Guid>
    where TUow : IBaseUnitOfWork
{
    public BaseEntityRepository(TDbContext dbContext, IMapper mapper, TUow uow) : base(dbContext, mapper, uow)
    {
    }
}