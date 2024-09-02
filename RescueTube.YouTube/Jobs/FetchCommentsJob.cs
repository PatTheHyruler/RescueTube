using Hangfire;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Jobs.Filters;
using RescueTube.Core.Utils;
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

    [SkipConcurrent("yt:fetch-video-comments:{0}")]
    [RescheduleConcurrentExecution("yt:fetch-video-comments")]
    public async Task FetchVideoComments(Guid videoId, CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        await _commentService.UpdateComments(videoId, ct);
        await _commentService.DataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }

    [RescheduleConcurrentExecution("yt:queue-fetch-video-comments")]
    public async Task QueueVideosForCommentFetch(CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        var lastCommentsFetchCutoff = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(1));
        var addedToArchiveAtCutoff = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(1));
        var videoIds = _commentService.DbCtx.Videos
            .Where(v => (v.LastCommentsFetch < lastCommentsFetchCutoff
                         || v.LastCommentsFetch == null)
                        && v.AddedToArchiveAt < addedToArchiveAtCutoff
                        && v.Platform == EPlatform.YouTube)
            .Select(v => v.Id)
            .AsAsyncEnumerable().WithCancellation(ct);

        await foreach (var videoId in videoIds)
        {
            _backgroundJobClient.Enqueue<FetchCommentsJob>(x =>
                x.FetchVideoComments(videoId, default));
        }
        transaction.Complete();
    }
}