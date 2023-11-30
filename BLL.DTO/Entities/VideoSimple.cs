using Domain.Base;
using Domain.Entities;
using Domain.Entities.Localization;
using Domain.Enums;

namespace BLL.DTO.Entities;

public class VideoSimple : BaseIdDbEntity
{
    public ICollection<TextTranslation>? Title { get; set; }
    public ICollection<TextTranslation>? Description { get; set; }

    public List<Image> Thumbnails { get; set; } = default!;
    public Image? Thumbnail { get; set; }

    public TimeSpan? Duration { get; set; }

    public EPlatform Platform { get; set; }
    public string IdOnPlatform { get; set; } = default!;

    public ICollection<AuthorSimple> Authors { get; set; } = default!;
    public DateTime? CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime AddedToArchiveAt { get; set; }

    public string? Url { get; set; }
    public string? EmbedUrl { get; set; }
}