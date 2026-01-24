using System.Linq.Expressions;

namespace RescueTube.Core.Utils.Pagination;

public class OrderingConfiguration<TEntity>
{
    public required IOrderByProperty[] DefaultOrdering { get; init; }
    public required IOrderByProperty RequiredProperty { get; init; }
    public required Dictionary<string, Expression<Func<TEntity, dynamic?>>> PropertyMap { get; init; }
}