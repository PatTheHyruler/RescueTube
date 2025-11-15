using System.Linq.Expressions;
using LinqKit;
using RescueTube.Core.Data;
using RescueTube.Core.Data.Specifications;
using RescueTube.Core.DataFetches;
using RescueTube.Domain.Contracts;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.DAL.EF.Specifications;

public class DataFetchSpecification : IDataFetchSpecification
{
    private readonly TimeProvider _timeProvider;
    private readonly AppDbContext _dbContext;

    public DataFetchSpecification(TimeProvider timeProvider, AppDbContext dbContext)
    {
        _timeProvider = timeProvider;
        _dbContext = dbContext;
    }

    public Expression<Func<DataFetch, Video, bool>> IsVideoDataFetch =>
        (d, v) => d.VideoId == v.Id;

    private static Expression<Func<DataFetch, Playlist, bool>> IsPlaylistDataFetch =>
        (d, p) => d.PlaylistId == p.Id;

    private static Expression<Func<DataFetch, Author, bool>> IsAuthorDataFetch =>
        (d, a) => d.AuthorId == a.Id;

    public Expression<Func<Author, bool>> ShouldFetchAuthorData(DataFetchDefinition dataFetchDefinition, DataFetchJobSettings dataFetchJobSettings)
    {
        return ShouldFetchData(IsAuthorDataFetch, dataFetchDefinition, dataFetchJobSettings);
    }

    public Expression<Func<Playlist, bool>> ShouldFetchPlaylistData(DataFetchDefinition dataFetchDefinition, DataFetchJobSettings dataFetchJobSettings)
    {
        return ShouldFetchData(IsPlaylistDataFetch, dataFetchDefinition, dataFetchJobSettings);
    }

    public Expression<Func<Video, bool>> ShouldFetchVideoData(DataFetchDefinition dataFetchDefinition, DataFetchJobSettings dataFetchJobSettings,
        Expression<Func<Video, bool>> allowRegularFetchesPredicate)
    {
        return IsDataFetchAllowed(IsVideoDataFetch, dataFetchDefinition).And(
            allowRegularFetchesPredicate.And(HasNoTooRecentDataFetches(IsVideoDataFetch, dataFetchDefinition, dataFetchJobSettings))
                .Or(HasNoSuccessDataFetchesAndNotBlockedByFailure(IsVideoDataFetch, dataFetchDefinition, dataFetchJobSettings)));
    }

    private Expression<Func<TEntity, bool>> ShouldFetchData<TEntity>(
        Expression<Func<DataFetch, TEntity, bool>> isEntityDataFetch,
        DataFetchDefinition dataFetchDefinition, DataFetchJobSettings dataFetchJobSettings)
        where TEntity : IIdDatabaseEntity, IPlatformEntity
    {
        return IsDataFetchAllowed(isEntityDataFetch, dataFetchDefinition)
            .And(HasNoTooRecentDataFetches(isEntityDataFetch, dataFetchDefinition, dataFetchJobSettings));
    }

    public Expression<Func<DataFetch, bool>> IsOngoing(DateTimeOffset currentTime)
    {
        var pendingDataFetchCutoff = currentTime.AddMinutes(-5);
        return df => df.Status == DataFetchStatus.Started &&
                     df.LastHeartbeatReceivedAt != null &&
                     df.LastHeartbeatReceivedAt >= pendingDataFetchCutoff;
    }

    private Expression<Func<TEntity, bool>> IsDataFetchAllowed<TEntity>(
        Expression<Func<DataFetch, TEntity, bool>> isEntityDataFetch, DataFetchDefinition dataFetchDefinition)
        where TEntity : IPlatformEntity
    {
        var currentTime = _timeProvider.GetUtcNow();
        return e =>
            e.Platform == dataFetchDefinition.Platform
            && !_dbContext.DataFetches.Any(df =>
                isEntityDataFetch.Invoke(df, e) &&
                IsOngoing(currentTime).Invoke(df));
    }

    private Expression<Func<TEntity, bool>> HasNoTooRecentDataFetches<TEntity>(
        Expression<Func<DataFetch, TEntity, bool>> isEntityDataFetch,
        DataFetchDefinition dataFetchDefinition, DataFetchJobSettings dataFetchJobSettings)
    {
        var now = _timeProvider.GetUtcNow();
        var successCutoff = now.Subtract(dataFetchJobSettings.SuccessCutoffOffset);
        var failureCutoff = now.Subtract(dataFetchJobSettings.FailureCutoffOffset);
        return e => !_dbContext.DataFetches
            .AsExpandable()
            .Where(d => isEntityDataFetch.Invoke(d, e))
            .Any(d => IsTooRecent(
                dataFetchDefinition.Source,
                dataFetchDefinition.Type,
                successCutoff,
                failureCutoff).Invoke(d));
    }

    private Expression<Func<TEntity, bool>> HasNoSuccessDataFetchesAndNotBlockedByFailure<TEntity>(
        Expression<Func<DataFetch, TEntity, bool>> isEntityDataFetch,
        DataFetchDefinition dataFetchDefinition, DataFetchJobSettings dataFetchJobSettings)
    {
        var now = _timeProvider.GetUtcNow();
        var successCutoff = DateTimeOffset.MinValue;
        var failureCutoff = now.Subtract(dataFetchJobSettings.FailureCutoffOffset);
        return e => !_dbContext.DataFetches
            .Where(d => isEntityDataFetch.Invoke(d, e))
            .Any(d => IsTooRecent(
                dataFetchDefinition.Source,
                dataFetchDefinition.Type,
                successCutoff,
                failureCutoff).Invoke(d));
    }

    private static Expression<Func<DataFetch, bool>> IsTooRecent(
        string source, string type, DateTimeOffset successCutoff, DateTimeOffset failureCutoff)
    {
        return d =>
            d.Source == source
            && d.Type == type
            && (
                (d.Status == DataFetchStatus.Succeeded && d.StartedAt > successCutoff)
                || (d.Status == DataFetchStatus.Failed && d.StartedAt > failureCutoff)
            );
    }
}