using System.Linq.Expressions;
using Hangfire;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.JobOrchestration;
using RescueTube.Core.Services;
using RescueTube.Domain.Entities;
using RescueTube.YouTube.Services;

namespace RescueTube.YouTube.Jobs.DataFetch;

public class FetchYouTubeExplodeAuthorDataJob : EntityDataFetchJobBase<Author, FetchYouTubeExplodeAuthorDataJob>, IJob
{
    public static string RecurringJobId => "yt:fetch-yt-explode-author-data";

    private static readonly DataFetchDefinition DataFetchDefinition = YouTubeConstants.DataFetches.YouTubeExplode.Channel;

    public static JobDefinition JobDefinition { get; } = new JobDefinition<FetchYouTubeExplodeAuthorDataJob>
    {
        IsArchivalJob = true,
        DefaultSettings = new()
        {
            JobId = RecurringJobId,
            Cron = "*/10 * * * *", // Every 10th minute
            IsEnabled = true,
            DataFetchJobSettings = new()
            {
                SuccessCutoffOffset = TimeSpan.FromDays(10),
                FailureCutoffOffset = TimeSpan.FromDays(1),
            },
        },
        DataFetchDefinition = DataFetchDefinition,
    };

    private readonly YouTubeServices _youTubeServices;
    private readonly TimeProvider _timeProvider;

    public FetchYouTubeExplodeAuthorDataJob(YouTubeServices youTubeServices, IDataUow dataUow,
        ILogger<FetchYouTubeExplodeAuthorDataJob> logger, TimeProvider timeProvider, IBackgroundJobClientV2 backgroundJobClient, IRecurringJobsService recurringJobsService)
        : base(dataUow, logger, DataFetchDefinition, backgroundJobClient, recurringJobsService)
    {
        _youTubeServices = youTubeServices;
        _timeProvider = timeProvider;
    }

    protected override Expression<Func<Author, bool>> GetFilterExpression(DataFetchJobSettings dataFetchJobSettings)
        => DataUow.DataFetches.ShouldFetchAuthorData(DataFetchDefinition, dataFetchJobSettings);

    public override async Task<EntityDataFetchResult> FetchEntityDataAsync(Guid entityId, CancellationToken ct)
    {
        if (AuthorService.LastYtExplodeRateLimitHit > _timeProvider.GetUtcNow().Subtract(TimeSpan.FromHours(1)))
        {
            Logger.LogDebug("Skipping YouTubeExplode data fetch, last rate limit was hit at {LastRateLimitHit}",
                AuthorService.LastYtExplodeRateLimitHit);
            return EntityDataFetchResult.Throttled;
        }

        await _youTubeServices.AuthorService.TryUpdateWithYouTubeExplodeDataAsync(entityId, ct);
        return EntityDataFetchResult.Completed;
    }
}