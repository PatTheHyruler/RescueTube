using System.Linq.Expressions;
using Hangfire;
using Hangfire.Server;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.JobOrchestration;
using RescueTube.Domain.Contracts;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.DataFetches;

public abstract class EntityDataFetchJobBase<TEntity, TJob> : IJobBase, IEntityDataFetchJob
    where TEntity : class, IIdDatabaseEntity, IPlatformEntity
    where TJob : EntityDataFetchJobBase<TEntity, TJob>, IJob
{
    protected readonly IDataUow DataUow;
    protected readonly ILogger Logger;
    private readonly IBackgroundJobClientV2 _backgroundJobClient;

    private DataFetchDefinition DataFetchDefinition { get; }

    private static string RecurringJobId => TJob.RecurringJobId;

    protected EntityDataFetchJobBase(IDataUow dataUow, ILogger logger, DataFetchDefinition dataFetchDefinition, IBackgroundJobClientV2 backgroundJobClient)
    {
        if (!IsValidEntityType(dataFetchDefinition.EntityType))
        {
            throw new ArgumentException($"Invalid entity type {dataFetchDefinition.EntityType} for type {typeof(TEntity).FullName}");
        }
        DataUow = dataUow;
        Logger = logger;
        DataFetchDefinition = dataFetchDefinition;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task RunAsync(PerformContext performContext, CancellationToken ct)
    {
        var entityIds = await DataUow.Ctx.Set<TEntity>()
            .AsExpandable()
            .Where(FilterExpression)
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .Take(2)
            .ToArrayAsync(ct);
        if (entityIds is not [var entityId, .. var nextIds])
        {
            return;
        }

        Logger.LogInformation(
            "Executing data fetch {Type} from {Source} for {Platform} {EntityType} {EntityId}",
            DataFetchDefinition.Type, DataFetchDefinition.Source, DataFetchDefinition.Platform,
            DataFetchDefinition.EntityType, entityId);
        var result = await FetchEntityDataAsync(entityId, ct);

        if (nextIds.Length > 0 && result is not EntityDataFetchResult.Throttled)
        {
            _backgroundJobClient.ContinueJobWith<IRecurringJobManagerV2>(performContext.BackgroundJob.Id,
                r => r.Trigger(RecurringJobId));
        }
    }

    protected abstract Expression<Func<TEntity, bool>> FilterExpression { get; }

    public abstract Task<EntityDataFetchResult> FetchEntityDataAsync(Guid entityId, CancellationToken ct);

    private static bool IsValidEntityType(EEntityType entityType)
    {
        var genericType = typeof(TEntity);
        return entityType switch
        {
            EEntityType.Video when genericType == typeof(Video) => true,
            EEntityType.Author when genericType == typeof(Author) => true,
            EEntityType.Playlist when genericType == typeof(Playlist) => true,
            _ => false,
        };
    }
}