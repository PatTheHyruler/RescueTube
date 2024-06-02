using System.Linq.Expressions;

namespace RescueTube.Core.Data.Pagination;

public record SortBehaviour<TEntity>(Expression<Func<TEntity, dynamic>> OrderExpression, bool Descending)
{
    public static implicit operator SortBehaviour<TEntity>((Expression<Func<TEntity, dynamic>> orderExpression,
        bool descending) tuple) => new(tuple.orderExpression, tuple.descending);
}