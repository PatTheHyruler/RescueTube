using System.Collections.Immutable;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using RescueTube.Core.Jobs;
using RescueTube.Core.Utils;

namespace RescueTube.Jobs.DAL.Postgres;

public class PostgresJobStorageAccessor : IJobStorageAccessor
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresJobStorageAccessor([FromKeyedServices(typeof(PostgresJobStorageAccessor))] NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<IImmutableSet<Guid>> GetActiveVideoFetchJobVideoIdsAsync(CancellationToken ct)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(ct);

        const string jobType = "RescueTube.YouTube.Jobs.FetchVideoDataJob, RescueTube.YouTube";
        const string methodName = "FetchVideoData";

        var command = new CommandDefinition(
            """
            SELECT j.arguments->>0 FROM hangfire.job j
            WHERE j.invocationdata->>'Type' = @JobType AND j.invocationdata->>'Method' = @Method AND j.statename IN ('Enqueued', 'Awaiting', 'Processing');
            """,
            new { JobType = jobType, Method = methodName },
            cancellationToken: ct
        );

        var jobIdStrings = await connection.QueryAsync<string?>(command);
        var jobIds = jobIdStrings
                .Select(x =>
                x is null
                    ? null
                    : Guid.TryParse(x.AsSpan(1, x.Length - 2), out var guid)
                        ? guid
                        : (Guid?)null)
            .WhereNotNull();

        return jobIds.ToImmutableHashSet();
    }
}