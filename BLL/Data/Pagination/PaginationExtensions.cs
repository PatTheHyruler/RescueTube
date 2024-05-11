using BLL.Utils.Pagination;
using BLL.Utils.Pagination.Contracts;

namespace BLL.Data.Pagination;

public static class PaginationExtensions
{
    public static IOrderedQueryable<TEntity> OrderBy<TEntity>(
        this IQueryable<TEntity> query,
        SortOptions sortOptions,
        SortBehaviour<TEntity> sortBehaviour)
    {
        if (sortOptions.Descending != null)
        {
            sortBehaviour = sortBehaviour with { Descending = sortOptions.Descending.Value };
        }

        return sortBehaviour.Descending
            ? query.OrderByDescending(sortBehaviour.OrderExpression)
            : query.OrderBy(sortBehaviour.OrderExpression);
    }

    public static IOrderedEnumerable<TEntity> OrderBy<TEntity>(
        this IEnumerable<TEntity> query,
        SortOptions sortOptions,
        SortBehaviour<TEntity> sortBehaviour)
    {
        if (sortOptions.Descending != null)
        {
            sortBehaviour = sortBehaviour with { Descending = sortOptions.Descending.Value };
        }

        return sortBehaviour.Descending
            ? query.OrderByDescending(sortBehaviour.OrderExpression.Compile())
            : query.OrderBy(sortBehaviour.OrderExpression.Compile());
    }

    public static IOrderedQueryable<TEntity> ThenBy<TEntity>(
        this IOrderedQueryable<TEntity> query,
        SortOptions sortOptions,
        SortBehaviour<TEntity> sortBehaviour)
    {
        if (sortOptions.Descending != null)
        {
            sortBehaviour = sortBehaviour with { Descending = sortOptions.Descending.Value };
        }

        return sortBehaviour.Descending
            ? query.ThenByDescending(sortBehaviour.OrderExpression)
            : query.ThenBy(sortBehaviour.OrderExpression);
    }

    public static IQueryable<TEntity> Paginate<TEntity>(
        this IQueryable<TEntity> query,
        IPaginationQuery paginationParams)
    {
        paginationParams.ConformValues();
        var skipAmount = paginationParams.GetSkipAmount();

        return query.Skip(skipAmount).Take(paginationParams.Limit);
    }

    public static IEnumerable<TEntity> Paginate<TEntity>(
        this IEnumerable<TEntity> query,
        IPaginationQuery paginationParams)
    {
        paginationParams.ConformValues();
        var skipAmount = paginationParams.GetSkipAmount();

        return query.Skip(skipAmount).Take(paginationParams.Limit);
    }
}