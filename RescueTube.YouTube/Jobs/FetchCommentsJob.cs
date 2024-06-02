using Hangfire;
using Microsoft.EntityFrameworkCore;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Services;

namespace RescueTube.YouTube.Jobs;

public class FetchCommentsJob
{
    private readonly CommentService _commentService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public FetchCommentsJob(CommentService commentService, IBackgroundJobClient backgroundJobClient)
    {
        _commentService = commentService;
        _backgroundJobClient = backgroundJobClient;
    }

    [DisableConcurrentExecution(timeoutInSeconds: 60 * 10)]
    public async Task FetchVideoComments(string videoIdOnPlatform, CancellationToken ct)
    {
        await _commentService.UpdateComments(videoIdOnPlatform, ct);
        await _commentService.DataUow.SaveChangesAsync(ct);
    }

    public async Task QueueVideosForCommentFetch(CancellationToken ct)
    {
        var lastCommentsFetchCutoff = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1));
        var addedToArchiveAtCutoff = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(1));
        var videoIdsOnPlatform = _commentService.DbCtx.Videos
            .Where(v => (v.LastCommentsFetch < lastCommentsFetchCutoff
                         || v.LastCommentsFetch == null)
                        && v.AddedToArchiveAt < addedToArchiveAtCutoff
                        && v.Platform == EPlatform.YouTube)
            .Select(v => v.IdOnPlatform)
            .AsAsyncEnumerable();

        await foreach (var videoIdOnPlatform in videoIdsOnPlatform)
        {
            _backgroundJobClient.Enqueue<FetchCommentsJob>(x =>
                x.FetchVideoComments(videoIdOnPlatform, default));
        }
    }
}