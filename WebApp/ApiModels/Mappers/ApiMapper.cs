using RescueTube.Core.DTO.Entities;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Localization;
using WebApp.Utils;

namespace WebApp.ApiModels.Mappers;

public static class ApiMapper
{
    public static VideoSimpleDtoV1 MapToVideoSimpleDtoV1(this VideoSimple srcVid, string? baseUrl)
    {
        return new VideoSimpleDtoV1
        {
            Id = srcVid.Id,
            Title = srcVid.Title.Select(MapTranslation).ToList(),
            Description = srcVid.Description.Select(MapTranslation).ToList(),

            Thumbnail = srcVid.Thumbnail?.MapImage(baseUrl),

            DurationSeconds = srcVid.Duration?.TotalSeconds,

            Platform = srcVid.Platform,
            IdOnPlatform = srcVid.IdOnPlatform,

            Authors = srcVid.Authors.Select(a => a.MapAuthorSimpleDtoV1(baseUrl)).ToList(),

            CreatedAt = srcVid.CreatedAt,
            PublishedAt = srcVid.PublishedAt,
            AddedToArchiveAt = srcVid.AddedToArchiveAt,

            ExternalUrl = srcVid.Url,
            EmbedUrl = srcVid.EmbedUrl,

            LastCommentsFetch = srcVid.LastCommentsFetch,
        };
    }

    public static AuthorSimpleDtoV1 MapAuthorSimpleDtoV1(this AuthorSimple src, string? baseUrl)
    {
        return new AuthorSimpleDtoV1
        {
            Id = src.Id,
            UserName = src.UserName,
            DisplayName = src.DisplayName,
            Platform = src.Platform,
            IdOnPlatform = src.IdOnPlatform,
            UrlOnPlatform = src.UrlOnPlatform,
            ProfileImages = src.ProfileImages.Select(i => i.MapImage(baseUrl)),
        };
    }

    public static ImageDtoV1 MapImage(this Image src, string? baseUrl)
    {
        return new ImageDtoV1
        {
            Id = src.Id,

            Platform = src.Platform,
            IdOnPlatform = src.IdOnPlatform,

            Key = src.Key,
            Quality = src.Quality,
            Ext = src.Ext,

            OriginalUrl = src.Url,
            LocalUrl = src.GetLocalUrl(baseUrl),
            Url = src.GetAnyUrl(baseUrl),

            LocalFilePath = src.LocalFilePath,

            Width = src.Width,
            Height = src.Height,
        };
    }

    public static TextTranslationDtoV1 MapTranslation(this TextTranslation src)
    {
        return new TextTranslationDtoV1
        {
            Id = src.Id,
            Content = src.Content,
            Culture = src.Culture,
            ValidSince = src.ValidSince,
            ValidUntil = src.ValidUntil,
        };
    }

    public static CommentDtoV1 MapComment(this CommentDto src, string? baseUrl)
    {
        return new CommentDtoV1
        {
            Id = src.Id,
            Platform = src.Platform,
            IdOnPlatform = src.IdOnPlatform,
            PrivacyStatusOnPlatform = src.PrivacyStatusOnPlatform,
            PrivacyStatus = src.PrivacyStatus,
            LastSuccessfulFetch = src.LastSuccessfulFetch?.MapDataFetchDtoV1(),
            LastUnSuccessfulFetch = src.LastUnSuccessfulFetch?.MapDataFetchDtoV1(),
            AddedToArchiveAt = src.AddedToArchiveAt,
            Author = src.Author.MapAuthorSimpleDtoV1(baseUrl),
            ConversationReplies = src.ConversationReplies?
                .Select(c => c.MapComment(baseUrl))
                .ToList(),
            DirectReplies = src.DirectReplies?
                .Select(c => c.MapComment(baseUrl))
                .ToList(),
            Content = src.Content,
            CreatedAt = src.CreatedAt,
            UpdatedAt = src.UpdatedAt,
            AuthorIsCreator = src.AuthorIsCreator,
            CreatedAtVideoTimeSeconds = src.CreatedAtVideoTimecode?.TotalSeconds,
            OrderIndex = src.OrderIndex,
            Statistics = src.Statistics?.MapCommentStatisticSnapshotDtoV1(),
            VideoId = src.VideoId,
        };
    }

    public static DataFetchDtoV1 MapDataFetchDtoV1(this DataFetch src) => new()
    {
        Id = src.Id,
        OccurredAt = src.OccurredAt,
        ShouldAffectValidity = src.ShouldAffectValidity,
        Source = src.Source,
        Success = src.Success,
        Type = src.Type,
    };

    public static CommentStatisticSnapshotDtoV1 MapCommentStatisticSnapshotDtoV1(this CommentStatisticSnapshotDto src)
    {
        return new CommentStatisticSnapshotDtoV1
        {
            LikeCount = src.LikeCount,
            DislikeCount = src.DislikeCount,
            ReplyCount = src.ReplyCount,
            IsFavorited = src.IsFavorited,
            ValidAt = src.ValidAt,
        };
    }
}