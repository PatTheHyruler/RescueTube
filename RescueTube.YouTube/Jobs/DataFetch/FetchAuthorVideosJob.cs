using System.Linq.Expressions;
using Hangfire;
using LinqKit;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.JobOrchestration;
using RescueTube.Domain.Entities;

namespace RescueTube.YouTube.Jobs.DataFetch;

public class FetchAuthorVideosJob : EntityDataFetchJobBase<Author, FetchAuthorVideosJob>, IEntityDataFetchJobWithDefinition, IJob
{
    public static string RecurringJobId => "yt:fetch-author-videos";

    public static readonly JobDefinition<FetchAuthorVideosJob> JobDefinition = new()
    {
        IsArchivalJob = true,
        DefaultSettings = new()
        {
            JobId = RecurringJobId,
            Cron = "*/10 * * * *", // Every 10th minute
            IsEnabled = true,
        },
    };

    private readonly YouTubeServices _youTubeServices;

    public FetchAuthorVideosJob(IDataUow dataUow, YouTubeServices youTubeServices, ILogger<FetchAuthorVideosJob> logger, IBackgroundJobClientV2 backgroundJobClient)
        : base(dataUow, logger, DataFetchJobDefinition.DataFetchDefinition, backgroundJobClient)
    {
        _youTubeServices = youTubeServices;
    }

    public static DataFetchJobDefinition DataFetchJobDefinition { get; } = new()
    {
        DataFetchDefinition = YouTubeConstants.DataFetches.YtDlp.ChannelVideos,
        SuccessCutoffOffset = TimeSpan.FromDays(10),
        FailureCutoffOffset = TimeSpan.FromDays(5),
    };

    protected override Expression<Func<Author, bool>> FilterExpression =>
        DataUow.DataFetches.ShouldFetchAuthorData(DataFetchJobDefinition)
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