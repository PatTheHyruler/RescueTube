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

    public Expression<Func<Video, bool>> ShouldFetchVideoData(
        DataFetchJobDefinition jobDefinition, Expression<Func<Video, bool>> allowRegularFetchesPredicate)
    {
        return IsDataFetchAllowed<Video>(jobDefinition).And(
            allowRegularFetchesPredicate.And(HasNoTooRecentDataFetches<Video>(jobDefinition))
                .Or(HasNoSuccessDataFetchesAndNotBlockedByFailure<Video>(jobDefinition)));
    }

    private Expression<Func<TEntity, bool>> ShouldFetchData<TEntity>(DataFetchJobDefinition jobDefinition)
        where TEntity : IIdDatabaseEntity, IPlatformEntity, IFetchable
    {
        return IsDataFetchAllowed<TEntity>(jobDefinition)
            .And(HasNoTooRecentDataFetches<TEntity>(jobDefinition));
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

    private Expression<Func<TEntity, bool>> HasNoTooRecentDataFetches<TEntity>(DataFetchJobDefinition jobDefinition)
        where TEntity : IFetchable
    {
        var now = _timeProvider.GetUtcNow();
        var successCutoff = now.Subtract(jobDefinition.SuccessCutoffOffset);
        var failureCutoff = now.Subtract(jobDefinition.FailureCutoffOffset);
        return e => !e.DataFetches!.Any(d => IsTooRecent(
            jobDefinition.DataFetchDefinition.Source,
            jobDefinition.DataFetchDefinition.Type,
            successCutoff,
            failureCutoff).Invoke(d));
    }

    private Expression<Func<TEntity, bool>> HasNoSuccessDataFetchesAndNotBlockedByFailure<TEntity>(DataFetchJobDefinition jobDefinition)
        where TEntity : IFetchable
    {
        var now = _timeProvider.GetUtcNow();
        var successCutoff = DateTimeOffset.MinValue;
        var failureCutoff = now.Subtract(jobDefinition.FailureCutoffOffset);
        return e => !e.DataFetches!.Any(d => IsTooRecent(
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
                (d.Success && d.OccurredAt > successCutoff)
                || (!d.Success && d.OccurredAt > failureCutoff)
            );
    }
}