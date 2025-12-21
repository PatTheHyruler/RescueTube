using System.Linq.Expressions;
using Hangfire;
using LinqKit;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.JobOrchestration;
using RescueTube.Core.Services;
using RescueTube.Domain.Entities;

namespace RescueTube.YouTube.Jobs.DataFetch;

public class FetchAuthorVideosJob : EntityDataFetchJobBase<Author, FetchAuthorVideosJob>, IJob
{
    public static string RecurringJobId => "yt:fetch-author-videos";

    private static readonly DataFetchDefinition DataFetchDefinition = YouTubeConstants.DataFetches.YtDlp.ChannelVideos;

    public static JobDefinition JobDefinition { get; } = new JobDefinition<FetchAuthorVideosJob>
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
                FailureCutoffOffset = TimeSpan.FromDays(5),
            },
        },
        DataFetchDefinition = DataFetchDefinition,
    };

    private readonly YouTubeServices _youTubeServices;

    public FetchAuthorVideosJob(IDataUow dataUow, YouTubeServices youTubeServices, ILogger<FetchAuthorVideosJob> logger, IBackgroundJobClientV2 backgroundJobClient, IRecurringJobsService recurringJobsService)
        : base(dataUow, logger, DataFetchDefinition, backgroundJobClient, recurringJobsService)
    {
        _youTubeServices = youTubeServices;
    }

    protected override Expression<Func<Author, bool>> GetFilterExpression(DataFetchJobSettings dataFetchJobSettings)
        => DataUow.DataFetches.ShouldFetchAuthorData(DataFetchDefinition, dataFetchJobSettings)
            .And(a =>
                a.ArchivalSettingsId != null
                && a.ArchivalSettings!.IsEnabledForArchival
                && a.ArchivalSettings!.ArchiveVideos);

    public override async Task<EntityDataFetchResult> FetchEntityDataAsync(Guid entityId, CancellationToken ct)
    {
        await _youTubeServices.AuthorService.TryFetchAuthorVideosAsync(authorId: entityId, ct: ct);
        return EntityDataFetchResult.Completed;
    }
}