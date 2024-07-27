using System.Linq.Expressions;
using LinqKit;
using RescueTube.Core.Contracts;
using RescueTube.Core.DTO.Entities;
using RescueTube.Core.Utils.ExpressionUtils;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Data.Mappers;

public class EntityMapper
{
    private readonly Dictionary<EPlatform, string?[]> _thumbnailQualities;
    private readonly Dictionary<EPlatform, string?[]> _thumbnailKeys;

    public EntityMapper(IEnumerable<IThumbnailComparer> thumbnailComparers)
    {
        _thumbnailQualities = new Dictionary<EPlatform, string?[]>();
        _thumbnailKeys = new Dictionary<EPlatform, string?[]>();
        foreach (var thumbnailComparer in thumbnailComparers)
        {
            if (thumbnailComparer.Qualities is { Length: > 0 })
            {
                _thumbnailQualities.Add(thumbnailComparer.Platform, thumbnailComparer.Qualities);
            }

            if (thumbnailComparer.Keys is { Length: > 0 })
            {
                _thumbnailKeys.Add(thumbnailComparer.Platform, thumbnailComparer.Keys);
            }
        }
    }


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

    public Expression<Func<Playlist, PlaylistDto>> ToPlaylistDto => pl => new PlaylistDto
    {
        Title = pl.Title!.Translations!,
        Description = pl.Description!.Translations!,

        Thumbnail = OrderThumbnails.Invoke(
                pl.PlaylistImages!.Where(pli => pli.ImageType == EImageType.Thumbnail)
                    .Select(pli => pli.Image!).AsQueryable()
            )
            .FirstOrDefault(),

        Creator = pl.Creator != null ? ToAuthorSimple.Invoke(pl.Creator) : null,

        PrivacyStatusOnPlatform = pl.PrivacyStatusOnPlatform,
        PrivacyStatus = pl.PrivacyStatus,

        Statistics = new CombinedStatisticSnapshotDto
        {
            CommentCount = pl.PlaylistStatisticSnapshots!
                .OrderByDescending(s => s.ValidAt)
                .Where(s => s.CommentCount != null)
                .Select(s => new StatisticSnapshotValueDto<long>
                {
                    Value = s.CommentCount!.Value,
                    ValidAt = s.ValidAt,
                })
                .FirstOrDefault(),
            ViewCount = pl.PlaylistStatisticSnapshots!
                .OrderByDescending(s => s.ValidAt)
                .Where(s => s.ViewCount != null)
                .Select(s => new StatisticSnapshotValueDto<long>
                {
                    Value = s.ViewCount!.Value,
                    ValidAt = s.ValidAt,
                })
                .FirstOrDefault(),
            LikeCount = pl.PlaylistStatisticSnapshots!
                .OrderByDescending(s => s.ValidAt)
                .Where(s => s.LikeCount != null)
                .Select(s => new StatisticSnapshotValueDto<long>
                {
                    Value = s.LikeCount!.Value,
                    ValidAt = s.ValidAt,
                })
                .FirstOrDefault(),
            DislikeCount = pl.PlaylistStatisticSnapshots!
                .OrderByDescending(s => s.ValidAt)
                .Where(s => s.DislikeCount != null)
                .Select(s => new StatisticSnapshotValueDto<long>
                {
                    Value = s.DislikeCount!.Value,
                    ValidAt = s.ValidAt,
                })
                .FirstOrDefault(),
        },

        Id = pl.Id,
        Platform = pl.Platform,
        IdOnPlatform = pl.IdOnPlatform,

        LastSuccessfulFetch = pl.DataFetches!
            .OrderByDescending(x => x.OccurredAt)
            .FirstOrDefault(x => x.Success && x.ShouldAffectValidity), // TODO: Make sure these actually compile to SQL
        LastUnSuccessfulFetch = pl.DataFetches!
            .OrderByDescending(x => x.OccurredAt)
            .FirstOrDefault(x => !x.Success && x.ShouldAffectValidity), // TODO: Make sure these actually compile to SQL

        AddedToArchiveAt = pl.AddedToArchiveAt,
        CreatedAt = pl.CreatedAt,
        UpdatedAt = pl.UpdatedAt,
    };

    public static Expression<Func<Comment, CommentDto>> ToCommentDto(int depth) => comment => new CommentDto
    {
        Id = comment.Id,
        Platform = comment.Platform,
        IdOnPlatform = comment.IdOnPlatform,
        PrivacyStatusOnPlatform = comment.PrivacyStatusOnPlatform,
        PrivacyStatus = comment.PrivacyStatus,
        LastSuccessfulFetch = comment.DataFetches!
            .OrderByDescending(x => x.OccurredAt)
            .FirstOrDefault(x => x.Success && x.ShouldAffectValidity), // TODO: Make sure these actually compile to SQL
        LastUnSuccessfulFetch = comment.DataFetches!
            .OrderByDescending(x => x.OccurredAt)
            .FirstOrDefault(x => !x.Success && x.ShouldAffectValidity), // TODO: Make sure these actually compile to SQL
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

    private Expression<Func<IQueryable<Image>, IOrderedQueryable<Image>>> OrderThumbnails
    {
        get
        {
            var selectQuality = ExpressionUtils.SelectValue<Image, EPlatform, string?[]>(i => i.Platform, _thumbnailQualities);
            var selectKey = ExpressionUtils.SelectValue<Image, EPlatform, string?[]>(i => i.Platform, _thumbnailKeys);
            return queryable => queryable
                .OrderByDescending(i => Array.IndexOf(selectQuality.Invoke(i), i.Quality))
                .ThenByDescending(i => Array.IndexOf(selectKey.Invoke(i), i.Key))
                .ThenByDescending(i => i.Width);
        }
    }

    public Expression<Func<Video, VideoSimple>> ToVideoSimple => v => new VideoSimple
    {
        Id = v.Id,

        Title = v.Title!.Translations!,
        Description = v.Description!.Translations!,

        Thumbnail = OrderThumbnails.Invoke(
                v.VideoImages!.Where(vi => vi.ImageType == EImageType.Thumbnail)
                    .Select(vi => vi.Image!).AsQueryable()
            )
            .FirstOrDefault(),
        Duration = v.Duration,

        Platform = v.Platform,
        IdOnPlatform = v.IdOnPlatform,

        Authors = v.VideoAuthors!.Select(va => ToAuthorSimple.Invoke(va.Author!)).ToList(),

        CreatedAt = v.CreatedAt,
        PublishedAt = v.PublishedAt,
        AddedToArchiveAt = v.AddedToArchiveAt,

        LastCommentsFetch = v.LastCommentsFetch,
    };
}