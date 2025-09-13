using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.Services;
using RescueTube.Domain.Entities;
using RescueTube.YouTube.Base;
using RescueTube.YouTube.Utils;
using YoutubeDLSharp.Metadata;
using Video = RescueTube.Domain.Entities.Video;

namespace RescueTube.YouTube.Services;

public class CommentService : BaseYouTubeService
{
    private readonly AppDbContext _dbCtx;
    private readonly ILogger<CommentService> _logger;
    private readonly EntityUpdateService _entityUpdateService;
    private readonly YouTubeServices _youTubeServices;

    public CommentService(AppDbContext dbCtx, ILogger<CommentService> logger, EntityUpdateService entityUpdateService, YouTubeServices youTubeServices)
    {
        _dbCtx = dbCtx;
        _logger = logger;
        _entityUpdateService = entityUpdateService;
        _youTubeServices = youTubeServices;
    }

    private DataFetch AddDataFetch(Guid videoId, DateTimeOffset commentsFetched, bool success)
    {
        return new DataFetch
        {
            OccurredAt = commentsFetched,
            Success = success,
            Type = YouTubeConstants.FetchTypes.YtDlp.Comments,
            Source = YouTubeConstants.FetchTypes.YtDlp.Source,
            ShouldAffectValidity = false,
            VideoId = videoId,
        };
    }

    private DataFetch AddDataFetch(Video video, DateTimeOffset commentsFetched, bool success)
    {
        var dataFetch = AddDataFetch(video.Id, commentsFetched, success);
        dataFetch.Video = video;
        return dataFetch;
    }

    // TODO: Use this
    public async Task UpdateComments(Guid videoId, CancellationToken ct)
    {
        var video = await _dbCtx.Videos
            .Where(v => v.Id == videoId)
            .Include(v => v.Comments!)
            .ThenInclude(v => v.CommentStatisticSnapshots)
            .SingleAsync(cancellationToken: ct);
        
        var videoData = await _youTubeServices.VideoService.FetchVideoDataYtdlAsync(video.IdOnPlatform, true, ct);
        if (videoData?.Comments == null)
        {
            AddDataFetch(video, DateTimeOffset.UtcNow, false);
            _logger.LogError("Failed to fetch comments for video {VideoId}", videoId);
            return;
        }

        var commentsFetched = DateTimeOffset.UtcNow;
        _logger.LogInformation(
            "Fetched {CommentsAmount} comments from YouTube for video {VideoId}",
            videoData.Comments.Length, video.Id
        );

        if (videoData.Comments.Length == 0 && videoData.CommentCount != 0)
        {
            AddDataFetch(video, commentsFetched, false);
            _logger.LogError(
                "Fetched 0 comments while reported comment count was {CommentCount}, assuming comments fetch error",
                videoData.CommentCount);
            return;
        }

        AddDataFetch(video, commentsFetched, true);
        video.LastCommentsFetch = commentsFetched;

        await UpdateComments(video, videoData.Comments);
    }

    private async Task UpdateComments(Video video, CommentData[] commentDatas)
    {
        if (video.Comments == null)
        {
            throw new ArgumentException("Video's comments should not be null when updating them", nameof(video));
        }

        // TODO: What to do if video has 20000 comments? Memory issues?
        var commentsWithoutParent = new List<(Comment Comment, string Parent)>();
        var commentsWithoutRoot = new List<(Comment Comment, string Root)>();

        var authorFetchArgs = commentDatas.Where(c => video.Comments
                .All(e => e.IdOnPlatform != c.ID))
            .DistinctBy(c => c.AuthorID)
            .Select(c => new AuthorFetchArg(
                c.AuthorID,
                () => c.ToDomainAuthor(YouTubeConstants.FetchTypes.YtDlp.Comments)));
        var addedOrFetchedAuthors = await _youTubeServices.AuthorService.AddOrGetAuthors(authorFetchArgs);

        var commentOrderIndex = 0L;

        foreach (var commentData in commentDatas)
        {
            var comment = commentData.ToDomainComment(YouTubeConstants.FetchTypes.YtDlp.Comments);
            comment.OrderIndex = ++commentOrderIndex;
            var existingDomainComment = video.Comments.SingleOrDefault(c => c.IdOnPlatform == commentData.ID);
            if (existingDomainComment != null)
            {
                _entityUpdateService.UpdateComment(existingDomainComment, comment, false);
                continue;
            }

            comment.AuthorId = addedOrFetchedAuthors.First(a => a.IdOnPlatform == commentData.AuthorID).Id;
            comment.VideoId = video.Id;
            if (commentData.Parent != "root")
            {
                var addedParentComment = video.Comments.SingleOrDefault(c => c.IdOnPlatform == commentData.Parent);
                if (addedParentComment == null)
                {
                    commentsWithoutParent.Add((comment, commentData.Parent));
                }
                else
                {
                    comment.ReplyTarget = addedParentComment;
                }

                var rootId = commentData.Parent.Split('.')[0];
                var addedRootComment = video.Comments.SingleOrDefault(c => c.IdOnPlatform == rootId);
                if (addedRootComment == null)
                {
                    commentsWithoutRoot.Add((comment, rootId));
                }
                else
                {
                    comment.ConversationRoot = addedRootComment;
                }
            }

            video.Comments.Add(comment);
            _dbCtx.AddIfTracked(comment);
        }

        foreach (var commentWithoutParent in commentsWithoutParent)
        {
            var parent = video.Comments.Single(c => c.IdOnPlatform == commentWithoutParent.Parent);
            commentWithoutParent.Comment.ReplyTarget = parent;
        }

        foreach (var commentWithoutRoot in commentsWithoutRoot)
        {
            var root = video.Comments.Single(c => c.IdOnPlatform == commentWithoutRoot.Root);
            commentWithoutRoot.Comment.ConversationRoot = root;
        }
    }
}