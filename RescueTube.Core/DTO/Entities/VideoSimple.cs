using Domain.Base;
using Domain.Entities;
using Domain.Entities.Localization;
using Domain.Enums;

namespace RescueTube.Core.DTO.Entities;

public class VideoSimple : BaseIdDbEntity
{
    public ICollection<TextTranslation>? Title { get; set; }
    public ICollection<TextTranslation>? Description { get; set; }

    public required List<Image> Thumbnails { get; set; }
    public Image? Thumbnail { get; set; }

    public TimeSpan? Duration { get; set; }

    public EPlatform Platform { get; set; }
    public required string IdOnPlatform { get; set; }

    public required ICollection<AuthorSimple> Authors { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime AddedToArchiveAt { get; set; }

    public string? Url { get; set; }
    public string? EmbedUrl { get; set; }
}