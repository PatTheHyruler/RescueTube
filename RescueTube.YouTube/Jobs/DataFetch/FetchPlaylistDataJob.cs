using System.Linq.Expressions;
using Hangfire;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.JobOrchestration;
using RescueTube.Core.Services;
using RescueTube.Domain.Entities;

namespace RescueTube.YouTube.Jobs.DataFetch;

public class FetchPlaylistDataJob : EntityDataFetchJobBase<Playlist, FetchPlaylistDataJob>, IJob
{
    public static string RecurringJobId => "yt:fetch-playlist-data";

    private static readonly DataFetchDefinition DataFetchDefinition = YouTubeConstants.DataFetches.YtDlp.Playlist;

    public static JobDefinition JobDefinition { get; } = new JobDefinition<FetchPlaylistDataJob>
    {
        IsArchivalJob = true,
        DefaultSettings = new()
        {
            JobId = RecurringJobId,
            Cron = "*/10 * * * *", // Every 10th minute
            IsEnabled = true,
            DataFetchJobSettings = new()
            {
                SuccessCutoffOffset = TimeSpan.FromDays(5),
                FailureCutoffOffset = TimeSpan.FromDays(1),
            },
        },
        DataFetchDefinition = DataFetchDefinition,
    };

    private readonly YouTubeServices _youTubeServices;

    public FetchPlaylistDataJob(IDataUow dataUow, YouTubeServices youTubeServices, ILogger<FetchPlaylistDataJob> logger, IBackgroundJobClientV2 backgroundJobClient, IRecurringJobsService recurringJobsService)
        : base(dataUow, logger, DataFetchDefinition, backgroundJobClient, recurringJobsService)
    {
        _youTubeServices = youTubeServices;
    }

    protected override Expression<Func<Playlist, bool>> GetFilterExpression(DataFetchJobSettings dataFetchJobSettings)
        => DataUow.DataFetches.ShouldFetchPlaylistData(DataFetchDefinition, dataFetchJobSettings);

    public override async Task<EntityDataFetchResult> FetchEntityDataAsync(Guid entityId, CancellationToken ct)
    {
        await _youTubeServices.PlaylistService.UpdatePlaylistAsync(entityId, ct);
        await DataUow.SaveChangesAsync(ct);
        return EntityDataFetchResult.Completed;
    }
}