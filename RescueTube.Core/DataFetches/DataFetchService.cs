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
        var dataFetch = AddDataFetch(definition);
        dataFetch.AuthorIdOnPlatform = author.IdOnPlatform;
        await _dbCtx.SaveChangesAsync(ct);

        return dataFetch;
    }

    public DataFetch AddDataFetch(DataFetchDefinition definition)
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

        _dbCtx.DataFetches.Add(dataFetch);

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