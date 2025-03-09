using System.Linq.Expressions;
using RescueTube.Domain;
using RescueTube.Domain.Contracts;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Data.Specifications;

public interface IDataFetchSpecification
{
    public Expression<Func<DataFetch, bool>> IsTooRecent(
        string source, string type, DateTimeOffset successCutoff, DateTimeOffset failureCutoff);

    public Expression<Func<TEntity, bool>> ShouldFetchData<TEntity>(DataFetchJobDefinition dataFetchJobDefinition)
        where TEntity : IIdDatabaseEntity, IPlatformEntity, IFetchable;
}