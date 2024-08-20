using Hangfire;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.Jobs.Filters;
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

    [DisableConcurrentExecution(5 * 60)]
    public async Task EnqueueVideoDataFetchesRecurring(CancellationToken ct)
    {
        var videoIds = _dataUow.Ctx.Videos
            .AsExpandable()
            .Where(v => v.Platform == EPlatform.YouTube
                        && !v.DataFetches!.Any(d => _dataUow.DataFetches.IsTooRecent(
                            YouTubeConstants.FetchTypes.YtDlp.Source,
                            YouTubeConstants.FetchTypes.YtDlp.VideoPage,
                            DateTimeOffset.UtcNow.AddDays(-10),
                            DateTimeOffset.UtcNow.AddHours(-12)
                        ).Invoke(d)))
            .Select(v => v.Id);
        await foreach (var videoId in videoIds.AsAsyncEnumerable().WithCancellation(ct))
        {
            _backgroundJobClient.Enqueue<FetchVideoDataJob>(x =>
                x.FetchVideoData(videoId, default));
        }
    }

    [DisableConcurrentSameArgExecution(5 * 60)]
    public async Task FetchVideoData(Guid videoId, CancellationToken ct)
    {
        await _youTubeUow.VideoService.AddOrUpdateVideoAsync(videoId, ct);
        await _dataUow.SaveChangesAsync(ct);
    }
}