using RescueTube.Domain.Enums;

namespace WebApp.ApiModels;

public class CommentDtoV1
{
    public Guid Id { get; set; }

    public EPlatform Platform { get; set; }
    public required string IdOnPlatform { get; set; }
    public EPrivacyStatus? PrivacyStatusOnPlatform { get; set; }
    public bool IsAvailable { get; set; }
    public EPrivacyStatus PrivacyStatus { get; set; }
    public DateTimeOffset? LastFetchUnofficial { get; set; }
    public DateTimeOffset? LastSuccessfulFetchUnofficial { get; set; }
    public DateTimeOffset? LastFetchOfficial { get; set; }
    public DateTimeOffset? LastSuccessfulFetchOfficial { get; set; }
    public DateTimeOffset AddedToArchiveAt { get; set; }

    public required AuthorSimpleDtoV1 Author { get; set; }

    public ICollection<CommentDtoV1>? ConversationReplies { get; set; }
    public ICollection<CommentDtoV1>? DirectReplies { get; set; }

    public string? Content { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool? AuthorIsCreator { get; set; }

    public double? CreatedAtVideoTimeSeconds { get; set; }

    public long OrderIndex { get; set; }

    public CommentStatisticSnapshotDtoV1? Statistics { get; set; }

    public Guid VideoId { get; set; }
}