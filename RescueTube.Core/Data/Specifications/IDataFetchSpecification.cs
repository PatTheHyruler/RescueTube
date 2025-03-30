using System.Linq.Expressions;
using RescueTube.Core.DataFetches;
using RescueTube.Core.Jobs;
using RescueTube.Domain.Contracts;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Data.Specifications;

public interface IDataFetchSpecification
{
    public Expression<Func<Author, bool>> ShouldFetchAuthorData(DataFetchJobDefinition dataFetchJobDefinition);

    public Expression<Func<TEntity, bool>> ShouldFetchData<TEntity>(DataFetchJobDefinition dataFetchJobDefinition)
        where TEntity : IIdDatabaseEntity, IPlatformEntity, IFetchable;
}