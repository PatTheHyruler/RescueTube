using System.Linq.Expressions;

namespace DAL.EF.Pagination;

public record SortBehaviour<TEntity>(Expression<Func<TEntity, dynamic>> OrderExpression, bool Descending)
{
    public static implicit operator SortBehaviour<TEntity>((Expression<Func<TEntity, dynamic>> orderExpression,
        bool descending) tuple) => new(tuple.orderExpression, tuple.descending);
}