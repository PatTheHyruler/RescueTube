using RescueTube.Core.DTO.Entities;
using RescueTube.Core.DTO.Videos;
using RescueTube.Core.Services;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Identity;
using RescueTube.Domain.Entities.Localization;
using RescueTube.WebApi.ApiModels.Auth;
using RescueTube.WebApi.ApiModels.Playlists;
using RescueTube.WebApi.ApiModels.Statistics;
using RescueTube.WebApi.Utils;
using Riok.Mapperly.Abstractions;

namespace RescueTube.WebApi.ApiModels.Mappers;

[Mapper]
public static partial class ApiMapper
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
        StartedAt = src.StartedAt,
        StatusUpdatedAt = src.StatusUpdatedAt,
        Source = src.Source,
        Status = src.Status,
        Type = src.Type,
        Platform = src.Platform,

        VideoId = src.VideoId,
        AuthorId = src.AuthorId,
        PlaylistId = src.PlaylistId,
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

    public static VideoDownloadStatisticByPlatformDtoV1 MapVideoDownloadStatisticByPlatformDtoV1(
        this StatisticsPresentationService.VideoDownloadStatisticByPlatformDto src)
    {
        return new VideoDownloadStatisticByPlatformDtoV1
        {
            Platform = src.Platform,
            HasVideoFile = src.HasVideoFile,
            Count = src.Count,
        };
    }

    public static AuthorArchivalSettingsDtoV1 MapToAuthorArchivalSettingsDtoV1(this AuthorArchivalSettings src, Guid authorId)
    {
        return new()
        {
            Id = src.Id,
            AuthorId = authorId,
            IsEnabledForArchival = src.IsEnabledForArchival,
            ArchiveClips = src.ArchiveClips,
            ArchivePlaylists = src.ArchivePlaylists,
            ArchiveVideos = src.ArchiveVideos,
        };
    }

    public static VideoArchivalSettingsDtoV1 MapToVideoArchivalSettingsDtoV1(this VideoArchivalSettings src)
    {
        return new()
        {
            ShouldRegularlyFetchVideoData = src.ShouldRegularlyFetchVideoData,
            DownloadPriority = src.DownloadPriority,
        };
    }

    public static VideoSearchFilter MapToCoreVideoFilter(this VideoSearchFilterDtoV1? src)
    {
        return new()
        {
            Platform = null,
            Name = src?.NameQuery,
            Author = src?.AuthorQuery,
            AuthorIds = src?.AuthorIds,
        };
    }

    public static PlaylistSimpleDtoV1 MapToPlaylistSimpleDtoV1(this PlaylistDto src, string? baseUrl)
    {
        return new PlaylistSimpleDtoV1
        {
            Id = src.Id,
            Thumbnail = src.Thumbnail?.MapImage(baseUrl),
            Title = src.Title.Select(MapTranslation).ToArray(),
            Description = src.Description.Select(MapTranslation).ToArray(),
            VideosCount = src.VideosCount,
            Authors = src.Creator is not null ? [src.Creator.MapAuthorSimpleDtoV1(baseUrl)] : [], // TODO: Change playlists domain model to allow multiple authors
            UrlOnPlatform = src.UrlOnPlatform,
            Platform = src.Platform,
            IdOnPlatform = src.IdOnPlatform,
            AddedToArchiveAt = src.AddedToArchiveAt,
            CreatedAt = src.CreatedAt,
            UpdatedAt = src.UpdatedAt,
        };
    }

    public static PlaylistSimpleDtoV1 MapToPlaylistSimpleDtoV1(this PlaylistSimpleDto src, string? baseUrl)
    {
        return new PlaylistSimpleDtoV1
        {
            Id = src.Id,
            Thumbnail = src.Thumbnail?.MapImage(baseUrl),
            Title = src.Title.Select(MapTranslation).ToArray(),
            Description = src.Description.Select(MapTranslation).ToArray(),
            VideosCount = src.VideosCount,
            Authors = src.Creator is not null ? [src.Creator.MapAuthorSimpleDtoV1(baseUrl)] : [], // TODO: Change playlists domain model to allow multiple authors
            UrlOnPlatform = src.UrlOnPlatform,
            Platform = src.Platform,
            IdOnPlatform = src.IdOnPlatform,
            AddedToArchiveAt = src.AddedToArchiveAt,
            CreatedAt = src.CreatedAt,
            UpdatedAt = src.UpdatedAt,
        };
    }

    public static PlaylistItemDtoV1 MapToPlaylistItemDtoV1(this PlaylistItemDto<VideoSimple> src, string? baseUrl)
    {
        return new PlaylistItemDtoV1
        {
            Id = src.Id,
            Video = src.Video.MapToVideoSimpleDtoV1(baseUrl),
            Position = src.Position,
            AddedAt = src.AddedAt,
            RemovedAt = src.RemovedAt,
        };
    }

#pragma warning disable RMG020 // Source member is not mapped to any target member
    private static partial UserSimpleDtoV1 MapToUserSimpleDtoV1(this User src);

    private static partial SubmissionHandlingFailureDtoV1 MapToSubmissionHandlingFailureDtoV1(
        this SubmissionHandlingFailure src);
#pragma warning restore RMG020

    public static SubmissionDtoV1 MapToSubmissionDtoV1(this Submission src) => new()
    {
        Id = src.Id,
        Platform = src.Platform,
        IdOnPlatform = src.IdOnPlatform,
        IdType = src.IdType,
        EntityType = src.EntityType,
        Url = src.Url,
        AddedBy = src.AddedBy!.MapToUserSimpleDtoV1(),
        AddedAt = src.AddedAt,
        ApprovedBy = src.ApprovedBy?.MapToUserSimpleDtoV1(),
        ApprovedAt = src.ApprovedAt,
        GrantAccess = src.GrantAccess,
        CompletedAt = src.CompletedAt,
        VideoId = src.VideoId,
        PlaylistId = src.PlaylistId,
        AuthorId = src.AuthorId,
        Failures = src.Failures!.Select(MapToSubmissionHandlingFailureDtoV1).ToArray(),
    };
}