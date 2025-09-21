using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Data;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.DataFetches;

public class DataFetchService
{
    private readonly AppDbContext _dbCtx;
    private readonly TimeProvider _timeProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DataFetchService(AppDbContext dbCtx, TimeProvider timeProvider, IServiceScopeFactory serviceScopeFactory)
    {
        _dbCtx = dbCtx;
        _timeProvider = timeProvider;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<DataFetch> AddDataFetchAsync(DataFetchDefinition definition, Author author, CancellationToken ct)
    {
        if (definition.EntityType is not EEntityType.Author)
        {
            throw new ArgumentException($"DataFetch definition must be for {EEntityType.Author} - {definition}", nameof(definition));
        }

        return await AddDataFetchAsync(definition, author.IdOnPlatform, ct);
    }

    public async Task<DataFetch> AddDataFetchAsync(DataFetchDefinition definition, Video video, CancellationToken ct)
    {
        if (definition.EntityType is not EEntityType.Video)
        {
            throw new ArgumentException($"DataFetch definition must be for {EEntityType.Video} - {definition}", nameof(definition));
        }

        return await AddDataFetchAsync(definition, video.IdOnPlatform, ct);
    }

    public async Task<DataFetch> AddDataFetchAsync(DataFetchDefinition definition, string idOnPlatform, CancellationToken ct)
    {
        var dataFetch = new DataFetch
        {
            Platform = definition.Platform,
            Source = definition.Source,
            Type = definition.Type,
            OccurredAt = _timeProvider.GetUtcNow(),
            Status = DataFetchStatus.Starting,
            DataFetchResults = [],
        };

        switch (definition.EntityType)
        {
            case EEntityType.Video:
                dataFetch.VideoIdOnPlatform = idOnPlatform;
                break;
            case EEntityType.Author:
                dataFetch.AuthorIdOnPlatform = idOnPlatform;
                break;
            case EEntityType.Playlist:
                dataFetch.PlaylistIdOnPlatform = idOnPlatform;
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

        await dbCtx.DataFetches
            .Where(x => x.Id == dataFetch.Id)
            .ExecuteUpdateAsync(x => x
                .SetProperty(d => d.Status, status)
                .SetProperty(d => d.Message, message));

        dataFetch.Status = status;
        _dbCtx.Entry(dataFetch).Property(x => x.Status).IsModified = false;

        dataFetch.Message = message;
        _dbCtx.Entry(dataFetch).Property(x => x.Message).IsModified = false;
    }
}