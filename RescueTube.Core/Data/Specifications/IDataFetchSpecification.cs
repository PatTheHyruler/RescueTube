using System.Linq.Expressions;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Data.Specifications;

public interface IDataFetchSpecification
{
    Expression<Func<DataFetch, bool>> IsTooRecent(
        string source, string type, DateTimeOffset successCutoff, DateTimeOffset failureCutoff);
}