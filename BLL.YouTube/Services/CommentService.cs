using BLL.Services;
using BLL.YouTube.Base;
using BLL.YouTube.Extensions;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YoutubeDLSharp.Metadata;

namespace BLL.YouTube.Services;

public class CommentService : BaseYouTubeService
{
    private readonly EntityUpdateService _entityUpdateService;
    
    public CommentService(IServiceProvider services, ILogger<CommentService> logger, EntityUpdateService entityUpdateService) : base(services, logger)
    {
        _entityUpdateService = entityUpdateService;
    }

    public async Task UpdateComments(string videoIdOnPlatform, CancellationToken ct)
    {
        var videoData = await YouTubeUow.VideoService.FetchVideoDataYtdl(videoIdOnPlatform, true, ct);
        if (videoData == null)
        {
            Logger.LogError("Failed to fetch comments for video {VideoId}", videoIdOnPlatform);
            return;
        }
        var commentsFetched = DateTime.UtcNow;
        Logger.LogInformation(
            "Fetched {CommentsAmount} comments from YouTube for video {VideoId}",
            videoData.Comments.Length, videoIdOnPlatform
        );

        var video = await Ctx.Videos
            .Where(v => v.Platform == EPlatform.YouTube && v.IdOnPlatform == videoIdOnPlatform)
            .Include(v => v.Comments)
            .SingleOrDefaultAsync(cancellationToken: ct);

        if (video == null)
        {
            Logger.LogError("Video {IdOnPlatform} not found in archive", videoIdOnPlatform);
            return;
        }

        video.LastCommentsFetch = commentsFetched;

        await UpdateComments(video, videoData.Comments);
        Ctx.Videos.Update(video);
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
        
        foreach (var comment in video.Comments)
        {
            if (commentDatas.All(c => c.ID != comment.IdOnPlatform))
            {
                comment.DeletedAt = DateTime.UtcNow;
            }
        }

        var authorFetchArgs = commentDatas.Where(c => video.Comments
                .All(e => e.IdOnPlatform != c.ID))
            .DistinctBy(c => c.AuthorID)
            .Select(c => new AuthorFetchArg(c.AuthorID, c.ToDomainAuthor));
        var addedOrFetchedAuthors = await YouTubeUow.AuthorService.AddOrGetAuthors(authorFetchArgs);

        var commentOrderIndex = 0L;
        
        foreach (var commentData in commentDatas)
        {
            var comment = commentData.ToDomainComment();
            comment.OrderIndex = ++commentOrderIndex;
            var existingDomainComment = video.Comments.SingleOrDefault(c => c.IdOnPlatform == commentData.ID);
            if (existingDomainComment != null)
            {
                await _entityUpdateService.UpdateComment(existingDomainComment, comment);
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