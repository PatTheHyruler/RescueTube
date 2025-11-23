using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

    public DataFetchService(AppDbContext dbCtx, IDataFetchSpecification dataFetchSpecification, TimeProvider timeProvider, IServiceScopeFactory serviceScopeFactory)
    {
        _dbCtx = dbCtx;
        _dataFetchSpecification = dataFetchSpecification;
        _timeProvider = timeProvider;
        _serviceScopeFactory = serviceScopeFactory;
    }

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
        var hasStartedDataFetch = entityId is not null && await IsFetchingAsync(definition, entityId.Value, ct);
        if (hasStartedDataFetch)
        {
            return null;
        }

        var dataFetch = await AddDataFetchAsync(definition, entityId, ct);
        return new DataFetchScope(dataFetch, this, ct);
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

        var dataFetchEntry = _dbCtx.Entry(dataFetch);

        dataFetch.Status = status;
        var statusEntry = dataFetchEntry.Property(x => x.Status);
        statusEntry.OriginalValue = status;
        statusEntry.IsModified = false;

        dataFetch.Message = message;
        var messageEntry = _dbCtx.Entry(dataFetch).Property(x => x.Message);
        messageEntry.OriginalValue = message;
        messageEntry.IsModified = false;
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