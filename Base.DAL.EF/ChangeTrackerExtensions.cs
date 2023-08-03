using AutoMapper;
using Contracts.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Base.DAL.EF;

public static class ChangeTrackerExtensions
{
    public static TEntity? GetEntity<TEntity>(this ChangeTracker changeTracker, Guid id)
        where TEntity : class, IIdDatabaseEntity => GetEntity<TEntity, Guid>(changeTracker, id);

    public static TEntity? GetEntity<TEntity, TKey>(this ChangeTracker changeTracker, TKey id)
        where TEntity : class, IIdDatabaseEntity<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        return changeTracker.Entries<TEntity>().GetEntity(id);
    }

    public static TEntity? GetEntity<TEntity>(this IEnumerable<EntityEntry<TEntity>> entries, Guid id)
        where TEntity : class, IIdDatabaseEntity =>
        GetEntity<TEntity, Guid>(entries, id);

    public static TEntity? GetEntity<TEntity, TKey>(this IEnumerable<EntityEntry<TEntity>> entries, TKey id)
        where TEntity : class, IIdDatabaseEntity<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        return entries.FirstOrDefault(e => e.Entity.GetType() == typeof(TEntity) && e.Entity.Id.Equals(id))?.Entity;
    }

    public static TDomainEntity? GetTrackedEntity<TDomainEntity, TKey>(this DbSet<TDomainEntity> dbSet, TKey id)
        where TDomainEntity : class, IIdDatabaseEntity<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        return dbSet.Local.FirstOrDefault(e => e.Id.Equals(id));
    }

    public static TDomainEntity? GetTrackedEntity<TDomainEntity, TKey>(this DbContext dbContext, TKey id)
        where TDomainEntity : class, IIdDatabaseEntity<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        var previousAutoDetect = dbContext.ChangeTracker.AutoDetectChangesEnabled;
        dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
        var result = dbContext.ChangeTracker.Entries<TDomainEntity>().FirstOrDefault(e => e.Entity.Id.Equals(id))
            ?.Entity;
        dbContext.ChangeTracker.AutoDetectChangesEnabled = previousAutoDetect;
        return result;
    }

    public static TDomainEntity? GetTrackedEntity<TDomainEntity>(this DbContext dbContext, Guid id)
        where TDomainEntity : class, IIdDatabaseEntity => GetTrackedEntity<TDomainEntity, Guid>(dbContext, id);

    public static TCustomEntityCollection AttachIfNotAttached<TCustomEntityCollection, TCustomEntity, TDomainEntity,
        TKey>(this TCustomEntityCollection entities, IMapper mapper, DbContext dbContext)
        where TCustomEntityCollection : ICollection<TCustomEntity>
        where TCustomEntity : class, IIdDatabaseEntity<TKey>
        where TKey : struct, IEquatable<TKey>
        where TDomainEntity : class, IIdDatabaseEntity<TKey>
    {
        foreach (var entity in entities)
        {
            entity.AttachIfNotAttached<TCustomEntity, TDomainEntity, TKey>(mapper, dbContext);
        }

        return entities;
    }

    public static TCustomEntity? AttachIfNotAttached<TCustomEntity, TDomainEntity, TKey>(this TCustomEntity? entity,
        IMapper mapper, DbContext dbContext)
        where TCustomEntity : class, IIdDatabaseEntity<TKey>
        where TKey : struct, IEquatable<TKey>
        where TDomainEntity : class, IIdDatabaseEntity<TKey>
    {
        if (entity == null) return null;
        if (dbContext.GetTrackedEntity<TDomainEntity, TKey>(entity.Id) == null)
        {
            dbContext.AttachIfNotAttachedRecursive<TDomainEntity, TKey>(
                mapper.Map<TCustomEntity, TDomainEntity>(entity));
        }

        return entity;
    }

    private static void AttachIfNotAttachedRecursive<TEntity, TKey>(this DbContext dbContext, TEntity? entity,
        Dictionary<TKey, IIdDatabaseEntity<TKey>>? trackedEntities = null)
        where TEntity : class, IIdDatabaseEntity<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        if (entity == null) return;

        if (trackedEntities == null)
        {
            trackedEntities = new Dictionary<TKey, IIdDatabaseEntity<TKey>>();
            var entries = dbContext.ChangeTracker.Entries();
            foreach (var trackedEntry in entries)
            {
                if (trackedEntry.Entity is IIdDatabaseEntity<TKey> idEntity)
                {
                    trackedEntities[idEntity.Id] = idEntity;
                }
            }
        }

        if (trackedEntities.GetValueOrDefault(entity.Id) != null) return;

        trackedEntities[entity.Id] = entity;

        var entry = dbContext.Entry(entity);

        entry.State = EntityState.Unchanged;

        foreach (var navigationEntry in entry.Navigations)
        {
            if (navigationEntry is CollectionEntry collectionEntry)
            {
                if (collectionEntry.CurrentValue != null)
                {
                    foreach (var relatedEntity in collectionEntry.CurrentValue)
                    {
                        if (relatedEntity is IIdDatabaseEntity<TKey> relatedIdEntity)
                        {
                            dbContext.AttachIfNotAttachedRecursive(relatedIdEntity, trackedEntities);
                        }
                        else
                        {
                            dbContext.Attach(relatedEntity);
                        }
                    }
                }
            }
            else if (navigationEntry is ReferenceEntry referenceEntry)
            {
                var relatedEntity = referenceEntry.CurrentValue;
                if (relatedEntity is IIdDatabaseEntity<TKey> relatedIdEntity)
                {
                    dbContext.AttachIfNotAttachedRecursive(relatedIdEntity, trackedEntities);
                }
                else if (relatedEntity != null)
                {
                    dbContext.Attach(relatedEntity);
                }
            }
        }
    }
}