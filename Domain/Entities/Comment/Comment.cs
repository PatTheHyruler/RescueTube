using System.ComponentModel.DataAnnotations.Schema;
using Domain.Base;
using Domain.Contracts;
using Domain.Enums;

namespace Domain.Entities;

public class Comment : BaseIdDbEntity, IMainArchiveEntity
{
    public EPlatform Platform { get; set; }
    public string IdOnPlatform { get; set; } = default!;
    public EPrivacyStatus? PrivacyStatusOnPlatform { get; set; }
    public bool IsAvailable { get; set; }
    public EPrivacyStatus PrivacyStatus { get; set; }
    public DateTime? LastFetchUnofficial { get; set; }
    public DateTime? LastSuccessfulFetchUnofficial { get; set; }
    public DateTime? LastFetchOfficial { get; set; }
    public DateTime? LastSuccessfulFetchOfficial { get; set; }
    public DateTime AddedToArchiveAt { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Author? Author { get; set; }
    public Guid AuthorId { get; set; }
    public Video? Video { get; set; }
    public Guid VideoId { get; set; }

    [ForeignKey(nameof(ReplyTargetId))]
    public Comment? ReplyTarget { get; set; }
    public Guid? ReplyTargetId { get; set; }

    [ForeignKey(nameof(ConversationRootId))]
    public Comment? ConversationRoot { get; set; }
    public Guid? ConversationRootId { get; set; }

    public string? Content { get; set; }


    public ICollection<CommentStatisticSnapshot>? CommentStatisticSnapshots { get; set; }
    public bool? AuthorIsCreator { get; set; }

    public TimeSpan? CreatedAtVideoTimecode { get; set; }
    public DateTime? DeletedAt { get; set; }

    public long OrderIndex { get; set; }
    
    [InverseProperty(nameof(ReplyTarget))]
    public ICollection<Comment>? DirectReplies { get; set; }
    [InverseProperty(nameof(ConversationRoot))]
    public ICollection<Comment>? ConversationReplies { get; set; }

    public ICollection<CommentHistory>? CommentHistories { get; set; }
}