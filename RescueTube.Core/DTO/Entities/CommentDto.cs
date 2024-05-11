using RescueTube.Domain.Enums;

namespace RescueTube.Core.DTO.Entities;

public class CommentDto
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

    public required AuthorSimple Author { get; set; }

    public ICollection<CommentDto>? ConversationReplies { get; set; }
    public ICollection<CommentDto>? DirectReplies { get; set; }
    
    public string? Content { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool? AuthorIsCreator { get; set; }

    public TimeSpan? CreatedAtVideoTimecode { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    
    public long OrderIndex { get; set; }

    public CommentStatisticSnapshotDto? Statistics { get; set; }
}