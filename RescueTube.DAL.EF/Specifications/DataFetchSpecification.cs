using System.Linq.Expressions;
using LinqKit;
using RescueTube.Core.Data.Specifications;
using RescueTube.Core.DataFetches;
using RescueTube.Core.Jobs;
using RescueTube.Core.Services;
using RescueTube.Domain.Contracts;
using RescueTube.Domain.Entities;

namespace RescueTube.DAL.EF.Specifications;

public class DataFetchSpecification : IDataFetchSpecification
{
    private readonly DataFetchContext _dataFetchContext;
    private readonly TimeProvider _timeProvider;

    public DataFetchSpecification(DataFetchContext dataFetchContext, TimeProvider timeProvider)
    {
        _dataFetchContext = dataFetchContext;
        _timeProvider = timeProvider;
    }

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

    public Expression<Func<Author, bool>> ShouldFetchAuthorData(DataFetchJobDefinition dataFetchJobDefinition)
    {
        return a =>
            AuthorIsActiveAndConfiguredForVideoArchival.Invoke(a)
            && ShouldFetchData<Author>(dataFetchJobDefinition).Invoke(a);
    }

    private static Expression<Func<Author, bool>> AuthorIsActiveAndConfiguredForVideoArchival => a =>
        a.ArchivalSettingsId != null
        && a.ArchivalSettings!.Active
        && a.ArchivalSettings!.ArchiveVideos;

    public Expression<Func<TEntity, bool>> ShouldFetchData<TEntity>(DataFetchJobDefinition dataFetchJobDefinition)
        where TEntity : IIdDatabaseEntity, IPlatformEntity, IFetchable
    {
        var currentlyProcessingIdsOnPlatform = _dataFetchContext.GetCurrentlyFetchingEntityIdsOnPlatform(dataFetchJobDefinition.DataFetchDefinition)
            .AsEnumerable();
        var now = _timeProvider.GetUtcNow();
        var successCutoff = now.Subtract(dataFetchJobDefinition.SuccessCutoffOffset);
        var failureCutoff = now.Subtract(dataFetchJobDefinition.FailureCutoffOffset);
        return e =>
            e.Platform == dataFetchJobDefinition.DataFetchDefinition.Platform
            && !currentlyProcessingIdsOnPlatform.Contains(e.IdOnPlatform)
            && !e.DataFetches!.Any(d => IsTooRecent(
                dataFetchJobDefinition.DataFetchDefinition.Source,
                dataFetchJobDefinition.DataFetchDefinition.Type,
                successCutoff,
                failureCutoff
            ).Invoke(d));
    }
}