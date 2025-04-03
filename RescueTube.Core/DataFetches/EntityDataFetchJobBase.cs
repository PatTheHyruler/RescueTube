using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.JobOrchestration;
using RescueTube.Domain.Contracts;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.DataFetches;

public abstract class EntityDataFetchJobBase<TEntity> : IJob
    where TEntity : class, IIdDatabaseEntity, IPlatformEntity, IFetchable
{
    protected readonly IDataUow DataUow;
    protected readonly ILogger Logger;

    private DataFetchDefinition DataFetchDefinition { get; }

    protected EntityDataFetchJobBase(IDataUow dataUow, ILogger logger, DataFetchDefinition dataFetchDefinition)
    {
        if (!IsValidEntityType(dataFetchDefinition.EntityType))
        {
            throw new ArgumentException($"Invalid entity type {dataFetchDefinition.EntityType} for type {typeof(TEntity).FullName}");
        }
        DataUow = dataUow;
        Logger = logger;
        DataFetchDefinition = dataFetchDefinition;
    }

    public async Task<JobExecutionResult> RunAsync(CancellationToken ct)
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
            return JobExecutionResult.NothingToProcess;
        }

        Logger.LogInformation(
            "Executing data fetch {Type} from {Source} for {Platform} {EntityType} {EntityId}",
            DataFetchDefinition.Type, DataFetchDefinition.Source, DataFetchDefinition.Platform,
            DataFetchDefinition.EntityType, entityId);
        await FetchEntityDataAsync(entityId, ct);
        return nextIds.Length != 0 ? JobExecutionResult.HasMoreToProcess : JobExecutionResult.Succeeded;
    }

    protected abstract Expression<Func<TEntity, bool>> FilterExpression { get; }

    protected abstract Task FetchEntityDataAsync(Guid entityId, CancellationToken ct);

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