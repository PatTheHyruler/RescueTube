using Hangfire;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Utils;
using RescueTube.Domain;
using RescueTube.Domain.Entities;

namespace RescueTube.YouTube.Jobs;

public class FetchVideoDataJob
{
    private readonly IDataUow _dataUow;
    private readonly YouTubeUow _youTubeUow;
    private readonly ILogger<FetchVideoDataJob> _logger;

    public FetchVideoDataJob(IDataUow dataUow, YouTubeUow youTubeUow, ILogger<FetchVideoDataJob> logger)
    {
        _dataUow = dataUow;
        _youTubeUow = youTubeUow;
        _logger = logger;
    }

    private static readonly DataFetchJobDefinition JobDefinition = new(
        YouTubeConstants.DataFetches.YtDlp.VideoPage,
        successCutoffOffset: TimeSpan.FromDays(10),
        failureCutoffOffset: TimeSpan.FromDays(12));

    [AutomaticRetry(Attempts = 0)]
    [SkipConcurrent("yt:fetch-next-video-data")]
    public async Task FetchNextVideoDataAsync(CancellationToken ct)
    {
        var videoId = await _dataUow.Ctx.Videos
            .AsExpandable()
            .Where(_dataUow.DataFetches.ShouldFetchData<Video>(JobDefinition))
            .OrderBy(v => v.Id) // TODO: Better thing to order by
            .Select(v => v.Id)
            .FirstOrDefaultAsync(ct);
        if (videoId == Guid.Empty)
        {
            return;
        }
        await FetchVideoDataAsync(videoId, ct);
    }

    private async Task FetchVideoDataAsync(Guid videoId, CancellationToken ct)
    {
        using var logScope = _logger.BeginScope("Fetching video data for video {VideoId}", videoId);
        using var transaction = TransactionUtils.NewTransactionScope();
        await _youTubeUow.VideoService.UpdateVideoAsync(videoId, ct);
        await _dataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }
}