using System.ComponentModel.DataAnnotations.Schema;
using RescueTube.Domain.Base;
using RescueTube.Domain.Contracts;
using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Entities;

public class Comment : BaseIdDbEntity, IMainArchiveEntity
{
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

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public Author? Author { get; set; }
    public Guid AuthorId { get; set; }
    public Video? Video { get; set; }
    public Guid VideoId { get; set; }

    [ForeignKey(nameof(ReplyTargetId))] public Comment? ReplyTarget { get; set; }
    public Guid? ReplyTargetId { get; set; }

    [ForeignKey(nameof(ConversationRootId))]
    public Comment? ConversationRoot { get; set; }

    public Guid? ConversationRootId { get; set; }

    public string? Content { get; set; }


    public bool? AuthorIsCreator { get; set; }

    public TimeSpan? CreatedAtVideoTimecode { get; set; }

    public long OrderIndex { get; set; }

    public ICollection<CommentStatisticSnapshot>? CommentStatisticSnapshots { get; set; }

    [InverseProperty(nameof(ReplyTarget))] public ICollection<Comment>? DirectReplies { get; set; }

    [InverseProperty(nameof(ConversationRoot))]
    public ICollection<Comment>? ConversationReplies { get; set; }

    public ICollection<CommentHistory>? CommentHistories { get; set; }
}