using Domain.Enums;

namespace BLL.DTO.Entities;

public class CommentDto
{
    public Guid Id { get; set; }
    
    public EPlatform Platform { get; set; }
    public required string IdOnPlatform { get; set; }
    public EPrivacyStatus? PrivacyStatusOnPlatform { get; set; }
    public bool IsAvailable { get; set; }
    public EPrivacyStatus PrivacyStatus { get; set; }
    public DateTime? LastFetchUnofficial { get; set; }
    public DateTime? LastSuccessfulFetchUnofficial { get; set; }
    public DateTime? LastFetchOfficial { get; set; }
    public DateTime? LastSuccessfulFetchOfficial { get; set; }
    public DateTime AddedToArchiveAt { get; set; }

    public required AuthorSimple Author { get; set; }

    public ICollection<CommentDto>? ConversationReplies { get; set; }
    public ICollection<CommentDto>? DirectReplies { get; set; }
    
    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool? AuthorIsCreator { get; set; }

    public TimeSpan? CreatedAtVideoTimecode { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public long OrderIndex { get; set; }

    public CommentStatisticSnapshotDto? Statistics { get; set; }
}