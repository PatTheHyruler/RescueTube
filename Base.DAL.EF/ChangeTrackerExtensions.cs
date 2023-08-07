using System.Diagnostics;
using System.Linq.Expressions;
using Contracts.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Base.DAL.EF;

public static class ChangeTrackerExtensions
{
    public static EntityEntry<TEntity> GetUpdatableEntry<TEntity>(this DbContext dbContext, Guid id)
        where TEntity : class, IIdDatabaseEntity, new() =>
        dbContext.GetUpdatableEntry<TEntity, Guid>(id);

    public static EntityEntry<TEntity> GetUpdatableEntry<TEntity, TKey>(this DbContext dbContext, TKey id)
        where TEntity : class, IIdDatabaseEntity<TKey>, new()
        where TKey : struct, IEquatable<TKey>
    {
        var entry = dbContext.ChangeTracker.Entries<TEntity>().FirstOrDefault(e => e.Entity.Id.Equals(id));
        if (entry != null)
        {
            return entry;
        }

        entry = dbContext.Attach(new TEntity
        {
            Id = id,
        });
        Debug.Assert(entry.State == EntityState.Unchanged);
        return entry;
    }

    public static void Update<TEntity>(this EntityEntry<TEntity> entry, TEntity values,
        params Expression<Func<TEntity, object?>>[] changedPropertyExpressions)
        where TEntity : class
    {
        foreach (var changedPropertyExpression in changedPropertyExpressions)
        {
            var property = entry.Property(changedPropertyExpression);
            var newValue = changedPropertyExpression.Compile()(values);
            var changed = newValue == null && property.Metadata.IsNullable;
            if (!changed)
            {
                changed = !property.Metadata.GetValueComparer()
                    .Equals(property.CurrentValue, changedPropertyExpression.Compile()(values));
            }
            if (changed)
            {
                property.CurrentValue = changedPropertyExpression.Compile()(values);
                property.IsModified = true;
            }
        }
    }
}