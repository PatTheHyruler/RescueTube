using RescueTube.Domain.Base;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Localization;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.DTO.Entities;

public class VideoSimple : BaseIdDbEntity
{
    public required ICollection<TextTranslation> Title { get; set; }
    public required ICollection<TextTranslation> Description { get; set; }

    public required List<Image> Thumbnails { get; set; }
    public Image? Thumbnail { get; set; }

    public TimeSpan? Duration { get; set; }

    public required EPlatform Platform { get; set; }
    public required string IdOnPlatform { get; set; }

    public required ICollection<AuthorSimple> Authors { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset AddedToArchiveAt { get; set; }

    public string? Url { get; set; }
    public string? EmbedUrl { get; set; }

    public DateTimeOffset? LastCommentsFetch { get; set; }
}