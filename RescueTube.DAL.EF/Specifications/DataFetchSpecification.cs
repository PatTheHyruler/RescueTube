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
    private readonly DataFetchContext _dataFetchContext;
    private readonly TimeProvider _timeProvider;
    private readonly AppDbContext _dbContext;

    public DataFetchSpecification(DataFetchContext dataFetchContext, TimeProvider timeProvider, AppDbContext dbContext)
    {
        _dataFetchContext = dataFetchContext;
        _timeProvider = timeProvider;
        _dbContext = dbContext;
    }

    public Expression<Func<DataFetch, Video, bool>> IsVideoDataFetch =>
        (d, v) => d.Platform == v.Platform && d.VideoIdOnPlatform == v.IdOnPlatform;

    private static Expression<Func<DataFetch, Playlist, bool>> IsPlaylistDataFetch =>
        (d, p) => d.Platform == p.Platform && d.PlaylistIdOnPlatform == p.IdOnPlatform;

    private static Expression<Func<DataFetch, Author, bool>> IsAuthorDataFetch =>
        (d, v) => d.Platform == v.Platform && d.AuthorIdOnPlatform == v.IdOnPlatform;

    public Expression<Func<Author, bool>> ShouldFetchAuthorData(DataFetchJobDefinition jobDefinition)
    {
        return ShouldFetchData(IsAuthorDataFetch, jobDefinition);
    }

    public Expression<Func<Playlist, bool>> ShouldFetchPlaylistData(DataFetchJobDefinition jobDefinition)
    {
        return ShouldFetchData(IsPlaylistDataFetch, jobDefinition);
    }

    public Expression<Func<Video, bool>> ShouldFetchVideoData(
        DataFetchJobDefinition jobDefinition, Expression<Func<Video, bool>> allowRegularFetchesPredicate)
    {
        return IsDataFetchAllowed<Video>(jobDefinition).And(
            allowRegularFetchesPredicate.And(HasNoTooRecentDataFetches(IsVideoDataFetch, jobDefinition))
                .Or(HasNoSuccessDataFetchesAndNotBlockedByFailure(IsVideoDataFetch, jobDefinition)));
    }

    private Expression<Func<TEntity, bool>> ShouldFetchData<TEntity>(
        Expression<Func<DataFetch, TEntity, bool>> isEntityDataFetch,
        DataFetchJobDefinition jobDefinition)
        where TEntity : IIdDatabaseEntity, IPlatformEntity
    {
        return IsDataFetchAllowed<TEntity>(jobDefinition)
            .And(HasNoTooRecentDataFetches(isEntityDataFetch, jobDefinition));
    }

    private Expression<Func<TEntity, bool>> IsDataFetchAllowed<TEntity>(DataFetchJobDefinition jobDefinition)
        where TEntity : IPlatformEntity
    {
        var currentlyProcessingIdsOnPlatform = _dataFetchContext.GetCurrentlyFetchingEntityIdsOnPlatform(jobDefinition.DataFetchDefinition)
            .AsEnumerable();
        return e =>
            e.Platform == jobDefinition.DataFetchDefinition.Platform
            && !currentlyProcessingIdsOnPlatform.Contains(e.IdOnPlatform);
    }

    private Expression<Func<TEntity, bool>> HasNoTooRecentDataFetches<TEntity>(
        Expression<Func<DataFetch, TEntity, bool>> isEntityDataFetch,
        DataFetchJobDefinition jobDefinition)
    {
        var now = _timeProvider.GetUtcNow();
        var successCutoff = now.Subtract(jobDefinition.SuccessCutoffOffset);
        var failureCutoff = now.Subtract(jobDefinition.FailureCutoffOffset);
        return e => !_dbContext.DataFetches
            .AsExpandable()
            .Where(d => isEntityDataFetch.Invoke(d, e))
            .Any(d => IsTooRecent(
                jobDefinition.DataFetchDefinition.Source,
                jobDefinition.DataFetchDefinition.Type,
                successCutoff,
                failureCutoff).Invoke(d));
    }

    private Expression<Func<TEntity, bool>> HasNoSuccessDataFetchesAndNotBlockedByFailure<TEntity>(
        Expression<Func<DataFetch, TEntity, bool>> isEntityDataFetch,
        DataFetchJobDefinition jobDefinition)
    {
        var now = _timeProvider.GetUtcNow();
        var successCutoff = DateTimeOffset.MinValue;
        var failureCutoff = now.Subtract(jobDefinition.FailureCutoffOffset);
        return e => !_dbContext.DataFetches
            .Where(d => isEntityDataFetch.Invoke(d, e))
            .Any(d => IsTooRecent(
                jobDefinition.DataFetchDefinition.Source,
                jobDefinition.DataFetchDefinition.Type,
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
                (d.Status != DataFetchStatus.Failed && d.OccurredAt > successCutoff)
                || (d.Status == DataFetchStatus.Failed && d.OccurredAt > failureCutoff)
            );
    }
}