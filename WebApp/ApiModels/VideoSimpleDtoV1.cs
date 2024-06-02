using RescueTube.Domain.Enums;

namespace WebApp.ApiModels;

public class VideoSimpleDtoV1
{
    public Guid Id { get; set; }
    public required List<TextTranslationDtoV1> Title { get; set; }
    public required List<TextTranslationDtoV1> Description { get; set; }

    public ImageDtoV1? Thumbnail { get; set; }

    public double? DurationSeconds { get; set; }

    public required EPlatform Platform { get; set; }
    public required string IdOnPlatform { get; set; }

    public required List<AuthorSimpleDtoV1> Authors { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset AddedToArchiveAt { get; set; }

    public string? ExternalUrl { get; set; }
    public string? EmbedUrl { get; set; }

    public DateTimeOffset? LastCommentsFetch { get; set; }
}