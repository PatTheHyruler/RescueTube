using System.Linq.Expressions;
using RescueTube.Core.Utils.Pagination;

namespace RescueTube.Core.Data.Pagination;

public static class PaginationExtensions
{
    public static IQueryable<TEntity> Paginate<TEntity>(
        this IQueryable<TEntity> query,
        IPaginationQuery paginationParams)
    {
        paginationParams = paginationParams.ToClamped();
        var skipAmount = paginationParams.GetSkipAmount();

        return query.Skip(skipAmount).Take(paginationParams.Limit);
    }

    private static IOrderedQueryable<TEntity>? OrderBy<TEntity>(
        this IQueryable<TEntity> query,
        ReadOnlySpan<IOrderByProperty> properties,
        Func<IOrderByProperty, Expression<Func<TEntity, dynamic?>>?> mapper)
    {
        IOrderedQueryable<TEntity>? orderedQuery = null;

        foreach (var property in properties)
        {
            var propertyExpression = mapper(property);
            if (propertyExpression is null)
            {
                continue;
            }

            if (orderedQuery is null)
            {
                orderedQuery = property.Descending
                    ? query.OrderByDescending(propertyExpression)
                    : query.OrderBy(propertyExpression);
            }
            else
            {
                orderedQuery = property.Descending
                    ? orderedQuery.ThenByDescending(propertyExpression)
                    : orderedQuery.ThenBy(propertyExpression);
            }
        }

        return orderedQuery;
    }

    public static IOrderedQueryable<TEntity> OrderByWithConfiguration<TEntity>(
        this IQueryable<TEntity> query,
        ReadOnlySpan<IOrderByProperty> userProperties,
        OrderingConfiguration<TEntity> config)
    {
        var orderedQuery = query.OrderBy(userProperties, property => 
            config.PropertyMap.GetValueOrDefault(property.PropertyName));

        if (orderedQuery is null)
        {
            return query.OrderBy(config.DefaultOrdering.AsSpan(), property =>
                               config.PropertyMap.GetValueOrDefault(property.PropertyName))
                           ?? throw new InvalidOperationException(
                               "No valid ordering properties found in configuration");
        }

        var hasRequiredProperty = userProperties.ToArray().Any(p => 
            p.PropertyName.Equals(config.RequiredProperty.PropertyName, StringComparison.OrdinalIgnoreCase));
        if (hasRequiredProperty)
        {
            return orderedQuery;
        }

        var requiredExpression = config.PropertyMap[config.RequiredProperty.PropertyName];
        orderedQuery = config.RequiredProperty.Descending
            ? orderedQuery.ThenByDescending(requiredExpression)
            : orderedQuery.ThenBy(requiredExpression);

        return orderedQuery;
    }
}