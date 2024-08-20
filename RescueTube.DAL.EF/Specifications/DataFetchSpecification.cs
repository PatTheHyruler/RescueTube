using System.Linq.Expressions;
using RescueTube.Core.Data.Specifications;
using RescueTube.Domain.Entities;

namespace RescueTube.DAL.EF.Specifications;

public class DataFetchSpecification : IDataFetchSpecification
{
    public Expression<Func<DataFetch, bool>> IsTooRecent(
        string source, string type, DateTimeOffset successCutoff, DateTimeOffset failureCutoff)
    {
        return d =>
            d.Source == source
            && d.Type == type
            && (
                (d.Success && d.OccurredAt > successCutoff)
                || (!d.Success && d.OccurredAt > failureCutoff)
            );
    }
}