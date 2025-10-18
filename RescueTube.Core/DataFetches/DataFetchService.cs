using System.Collections.Concurrent;
using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.Data.Specifications;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.DataFetches;

public class DataFetchService
{
    private readonly AppDbContext _dbCtx;
    private readonly IDataFetchSpecification _dataFetchSpecification;
    private readonly TimeProvider _timeProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<DataFetchService> _logger;

    public DataFetchService(AppDbContext dbCtx, IDataFetchSpecification dataFetchSpecification, TimeProvider timeProvider, IServiceScopeFactory serviceScopeFactory, ILogger<DataFetchService> logger)
    {
        _dbCtx = dbCtx;
        _dataFetchSpecification = dataFetchSpecification;
        _timeProvider = timeProvider;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    private static readonly ConcurrentDictionary<DataFetchDefinition, byte> DataFetchStartLocks = [];

    public async Task<DataFetchScope?> StartDataFetchAsync(DataFetchDefinition definition, Author author, CancellationToken ct)
    {
        if (definition.EntityType is not EEntityType.Author)
        {
            throw new ArgumentException($"DataFetch definition must be for {EEntityType.Author} - {definition}", nameof(definition));
        }

        return await StartDataFetchAsync(definition, author.Id, ct);
    }

    public async Task<DataFetchScope?> StartDataFetchAsync(DataFetchDefinition definition, Video video, CancellationToken ct)
    {
        if (definition.EntityType is not EEntityType.Video)
        {
            throw new ArgumentException($"DataFetch definition must be for {EEntityType.Video} - {definition}", nameof(definition));
        }

        return await StartDataFetchAsync(definition, video.Id, ct);
    }

    public async Task<DataFetchScope?> StartDataFetchAsync(DataFetchDefinition definition, Guid? entityId, CancellationToken ct)
    {
        // Short-lived lock, used to avoid race condition between IsFetching check and adding new DataFetch.
        // Not a distributed-safe check, and assumes that this is the only place that adds "Starting" DataFetches.
        var lockSuccessfullyAcquired = DataFetchStartLocks.TryAdd(definition, byte.MinValue);
        if (!lockSuccessfullyAcquired)
        {
            return null;
        }

        try
        {
            var hasStartedDataFetch = entityId is not null && await IsFetchingAsync(definition, entityId.Value, ct);
            if (hasStartedDataFetch)
            {
                return null;
            }

            var dataFetch = await AddDataFetchAsync(definition, entityId, ct);
            return new DataFetchScope(dataFetch, this, ct);
        }
        finally
        {
            var lockSuccessfullyReleased = DataFetchStartLocks.TryRemove(definition, out _);
            if (!lockSuccessfullyReleased)
            {
                _logger.LogError("Failed to release lock for {DataFetchDefinition}, {EntityId}", definition, entityId);
            }
        }
    }

    private async Task<DataFetch> AddDataFetchAsync(DataFetchDefinition definition, Guid? entityId, CancellationToken ct)
    {
        var now = _timeProvider.GetUtcNow();

        var dataFetch = new DataFetch
        {
            Platform = definition.Platform,
            Source = definition.Source,
            Type = definition.Type,
            StartedAt = now,
            LastHeartbeatReceivedAt = now,
            Status = DataFetchStatus.Started,
            DataFetchResults = [],
        };

        switch (definition.EntityType)
        {
            case EEntityType.Video:
                dataFetch.VideoId = entityId;
                break;
            case EEntityType.Author:
                dataFetch.AuthorId = entityId;
                break;
            case EEntityType.Playlist:
                dataFetch.PlaylistId = entityId;
                break;
            default:
                throw new ArgumentException($"Unknown/unsupported entity type {definition.EntityType} in DataFetch definition {definition}", nameof(definition));
        }

        _dbCtx.DataFetches.Add(dataFetch);
        await _dbCtx.SaveChangesAsync(ct);

        return dataFetch;
    }

    public async Task UpdateDataFetchStatusAsync(DataFetch dataFetch, DataFetchStatus status, string? message)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var dbCtx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = _timeProvider.GetUtcNow();

        await dbCtx.DataFetches
            .Where(x => x.Id == dataFetch.Id)
            .ExecuteUpdateAsync(x => x
                .SetProperty(d => d.Status, status)
                .SetProperty(d => d.Message, message)
                .SetProperty(d => d.StatusUpdatedAt, now));

        dataFetch.Status = status;
        _dbCtx.Entry(dataFetch).Property(x => x.Status).IsModified = false;

        dataFetch.Message = message;
        _dbCtx.Entry(dataFetch).Property(x => x.Message).IsModified = false;
    }

    public async Task SendDataFetchHeartbeatAsync(DataFetch dataFetch, CancellationToken ct)
    {
        if (dataFetch.Status is not DataFetchStatus.Started)
        {
            return;
        }

        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var dbCtx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = _timeProvider.GetUtcNow();
        await dbCtx.DataFetches
            .Where(x => x.Id == dataFetch.Id)
            .Where(x => x.Status == DataFetchStatus.Started)
            .Where(x => x.LastHeartbeatReceivedAt == null || x.LastHeartbeatReceivedAt <= now)
            .ExecuteUpdateAsync(x => x.SetProperty(df => df.LastHeartbeatReceivedAt, now), ct);
    }

    private async Task<bool> IsFetchingAsync(DataFetchDefinition definition, Guid entityId, CancellationToken ct)
    {
        var currentTime = _timeProvider.GetUtcNow();
        return await _dbCtx.DataFetches
            .AsExpandable()
            .Where(df =>
                df.Platform == definition.Platform &&
                df.Source == definition.Source &&
                df.Type == definition.Type &&
                _dataFetchSpecification.IsOngoing(currentTime).Invoke(df))
            .Where(IsEntityDataFetch(definition, entityId))
            .AnyAsync(ct);
    }

    private static Expression<Func<DataFetch, bool>> IsEntityDataFetch(DataFetchDefinition definition, Guid entityId)
    {
#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
        Expression<Func<DataFetch, bool>> isEntityDataFetch = definition.EntityType switch
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
        {
            EEntityType.Video => df => df.VideoId == entityId,
            EEntityType.Author => df => df.AuthorId == entityId,
            EEntityType.Playlist => df => df.PlaylistId == entityId,
        };
        return isEntityDataFetch;
    }

    public void CompleteDataFetch(DataFetch dataFetch)
    {
        dataFetch.Status = DataFetchStatus.Succeeded;
        dataFetch.StatusUpdatedAt = _timeProvider.GetUtcNow();
    }
}