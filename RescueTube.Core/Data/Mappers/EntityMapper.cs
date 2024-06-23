using System.Linq.Expressions;
using LinqKit;
using RescueTube.Core.DTO.Entities;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Data.Mappers;

public static class EntityMapper
{
    public static Expression<Func<Author, AuthorSimple>> ToAuthorSimple => author => new AuthorSimple
    {
        Id = author.Id,
        UserName = author.UserName,
        DisplayName = author.DisplayName,
        Platform = author.Platform,
        IdOnPlatform = author.IdOnPlatform,
        ProfileImages = author.AuthorImages!
            .Where(ai => ai.ImageType == EImageType.ProfilePicture)
            .Select(ai => ai.Image!)
            .ToList(),
        UrlOnPlatform = null,
    };

    public static Expression<Func<Comment, CommentDto>> ToCommentDto(int depth) => comment => new CommentDto
    {
        Id = comment.Id,
        Platform = comment.Platform,
        IdOnPlatform = comment.IdOnPlatform,
        PrivacyStatusOnPlatform = comment.PrivacyStatusOnPlatform,
        PrivacyStatus = comment.PrivacyStatus,
        LastFetchUnofficial = comment.LastFetchUnofficial,
        LastSuccessfulFetchUnofficial = comment.LastSuccessfulFetchUnofficial,
        LastFetchOfficial = comment.LastFetchOfficial,
        LastSuccessfulFetchOfficial = comment.LastSuccessfulFetchOfficial,
        AddedToArchiveAt = comment.AddedToArchiveAt,
        Author = ToAuthorSimple.Invoke(comment.Author!),
        ConversationReplies = depth < 1 
            ? comment.ConversationReplies!.Select(reply => ToCommentDto(depth + 1).Invoke(reply)).ToList()
            : null,
        DirectReplies = null,
        Content = comment.Content,
        CreatedAt = comment.CreatedAt,
        UpdatedAt = comment.UpdatedAt,
        AuthorIsCreator = comment.AuthorIsCreator,
        CreatedAtVideoTimecode = comment.CreatedAtVideoTimecode,
        OrderIndex = comment.OrderIndex,
        Statistics = GetLatestCommentStatisticSnapshotDto.Invoke(comment),
        VideoId = comment.VideoId,
    };

    public static Expression<Func<Comment, CommentStatisticSnapshotDto?>> GetLatestCommentStatisticSnapshotDto =>
        comment =>
            comment.CommentStatisticSnapshots!
                .OrderByDescending(s => s.ValidAt)
                .Select(s => new CommentStatisticSnapshotDto
                {
                    LikeCount = s.LikeCount,
                    DislikeCount = s.DislikeCount,
                    ReplyCount = s.ReplyCount,
                    IsFavorited = s.IsFavorited,
                    ValidAt = s.ValidAt,
                })
                .FirstOrDefault();
}