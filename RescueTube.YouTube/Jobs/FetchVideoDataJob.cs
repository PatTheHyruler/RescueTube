using Hangfire;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Utils;
using RescueTube.Domain.Enums;

namespace RescueTube.YouTube.Jobs;

public class FetchVideoDataJob
{
    private readonly IDataUow _dataUow;
    private readonly YouTubeUow _youTubeUow;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public FetchVideoDataJob(IDataUow dataUow, YouTubeUow youTubeUow, IBackgroundJobClient backgroundJobClient)
    {
        _dataUow = dataUow;
        _youTubeUow = youTubeUow;
        _backgroundJobClient = backgroundJobClient;
    }

    [RescheduleConcurrentExecution("yt:enqueue-video-data-fetches-recurring")]
    public async Task EnqueueVideoDataFetchesRecurring(CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        var videoIds = _dataUow.Ctx.Videos
            .AsExpandable()
            .Where(v => v.Platform == EPlatform.YouTube
                        && !v.DataFetches!.Any(d => _dataUow.DataFetches.IsTooRecent(
                            YouTubeConstants.FetchTypes.YtDlp.Source,
                            YouTubeConstants.FetchTypes.YtDlp.VideoPage,
                            DateTimeOffset.UtcNow.AddDays(-10),
                            DateTimeOffset.UtcNow.AddHours(-12)
                        ).Invoke(d)))
            .Select(v => v.Id)
            .AsAsyncEnumerable().WithCancellation(ct);
        await foreach (var videoId in videoIds)
        {
            _backgroundJobClient.Enqueue<FetchVideoDataJob>(x =>
                x.FetchVideoData(videoId, default));
        }
        transaction.Complete();
    }

    [SkipConcurrent("yt:fetch-video-data:{0}")]
    public async Task FetchVideoData(Guid videoId, CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        await _youTubeUow.VideoService.AddOrUpdateVideoAsync(videoId, ct);
        await _dataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }
}