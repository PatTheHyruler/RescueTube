using System.Linq.Expressions;
using LinqKit;
using RescueTube.Core.Data.Specifications;
using RescueTube.Core.DataFetches;
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

    public Expression<Func<Author, bool>> ShouldFetchAuthorData(DataFetchJobDefinition jobDefinition)
    {
        return ShouldFetchData<Author>(jobDefinition);
    }

    public Expression<Func<Playlist, bool>> ShouldFetchPlaylistData(DataFetchJobDefinition jobDefinition)
    {
        return ShouldFetchData<Playlist>(jobDefinition);
    }

    public Expression<Func<Video, bool>> ShouldFetchVideoData(DataFetchJobDefinition jobDefinition)
    {
        return ShouldFetchData<Video>(jobDefinition);
    }

    public Expression<Func<Author, bool>> AuthorIsActiveAndConfiguredForVideoArchival => a =>
        a.ArchivalSettingsId != null
        && a.ArchivalSettings!.Active
        && a.ArchivalSettings!.ArchiveVideos;

    private Expression<Func<TEntity, bool>> ShouldFetchData<TEntity>(DataFetchJobDefinition jobDefinition)
        where TEntity : IIdDatabaseEntity, IPlatformEntity, IFetchable
    {
        var currentlyProcessingIdsOnPlatform = _dataFetchContext.GetCurrentlyFetchingEntityIdsOnPlatform(jobDefinition.DataFetchDefinition)
            .AsEnumerable();
        var now = _timeProvider.GetUtcNow();
        var successCutoff = now.Subtract(jobDefinition.SuccessCutoffOffset);
        var failureCutoff = now.Subtract(jobDefinition.FailureCutoffOffset);
        return e =>
            e.Platform == jobDefinition.DataFetchDefinition.Platform
            && !currentlyProcessingIdsOnPlatform.Contains(e.IdOnPlatform)
            && !e.DataFetches!.Any(d => IsTooRecent(
                jobDefinition.DataFetchDefinition.Source,
                jobDefinition.DataFetchDefinition.Type,
                successCutoff,
                failureCutoff
            ).Invoke(d));
    }

    private static Expression<Func<DataFetch, bool>> IsTooRecent(
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