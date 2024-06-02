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

    public static Expression<Func<Comment, CommentDto>> ToCommentDto => comment => new CommentDto
    {
        Id = comment.Id,
        Platform = comment.Platform,
        IdOnPlatform = comment.IdOnPlatform,
        PrivacyStatusOnPlatform = comment.PrivacyStatusOnPlatform,
        IsAvailable = comment.IsAvailable,
        PrivacyStatus = comment.PrivacyStatus,
        LastFetchUnofficial = comment.LastFetchUnofficial,
        LastSuccessfulFetchUnofficial = comment.LastSuccessfulFetchUnofficial,
        LastFetchOfficial = comment.LastFetchOfficial,
        LastSuccessfulFetchOfficial = comment.LastSuccessfulFetchOfficial,
        AddedToArchiveAt = comment.AddedToArchiveAt,
        Author = ToAuthorSimple.Invoke(comment.Author!),
        ConversationReplies = comment.ConversationReplies!.Select(reply => new CommentDto
        {
            Id = reply.Id,
            Platform = reply.Platform,
            IdOnPlatform = reply.IdOnPlatform,
            PrivacyStatusOnPlatform = reply.PrivacyStatusOnPlatform,
            IsAvailable = reply.IsAvailable,
            PrivacyStatus = reply.PrivacyStatus,
            LastFetchUnofficial = reply.LastFetchUnofficial,
            LastSuccessfulFetchUnofficial = reply.LastSuccessfulFetchUnofficial,
            LastFetchOfficial = reply.LastFetchOfficial,
            LastSuccessfulFetchOfficial = reply.LastSuccessfulFetchOfficial,
            AddedToArchiveAt = reply.AddedToArchiveAt,
            Author = ToAuthorSimple.Invoke(reply.Author!),
            ConversationReplies = null,
            DirectReplies = null,
            Content = reply.Content,
            CreatedAt = reply.CreatedAt,
            UpdatedAt = reply.UpdatedAt,
            AuthorIsCreator = reply.AuthorIsCreator,
            CreatedAtVideoTimecode = reply.CreatedAtVideoTimecode,
            OrderIndex = reply.OrderIndex,
            Statistics = reply.CommentStatisticSnapshots!
                .OrderByDescending(s => s.ValidAt)
                .Select(s => new CommentStatisticSnapshotDto
                {
                    LikeCount = s.LikeCount,
                    DislikeCount = s.DislikeCount,
                    ReplyCount = s.ReplyCount,
                    IsFavorited = s.IsFavorited,
                    ValidAt = s.ValidAt,
                })
                .FirstOrDefault(),
            VideoId = reply.VideoId,
        }).ToList(),
        DirectReplies = null,
        Content = comment.Content,
        CreatedAt = comment.CreatedAt,
        UpdatedAt = comment.UpdatedAt,
        AuthorIsCreator = comment.AuthorIsCreator,
        CreatedAtVideoTimecode = comment.CreatedAtVideoTimecode,
        OrderIndex = comment.OrderIndex,
        Statistics = comment.CommentStatisticSnapshots!
            .OrderByDescending(s => s.ValidAt)
            .Select(s => new CommentStatisticSnapshotDto
            {
                LikeCount = s.LikeCount,
                DislikeCount = s.DislikeCount,
                ReplyCount = s.ReplyCount,
                IsFavorited = s.IsFavorited,
                ValidAt = s.ValidAt,
            })
            .FirstOrDefault(),
        VideoId = comment.VideoId,
    };
}