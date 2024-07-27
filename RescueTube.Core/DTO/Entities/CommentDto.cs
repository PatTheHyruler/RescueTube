using RescueTube.Domain.Contracts;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.DTO.Entities;

public class CommentDto : IFetchable
{
    public Guid Id { get; set; }

    public EPlatform Platform { get; set; }
    public required string IdOnPlatform { get; set; }
    public EPrivacyStatus? PrivacyStatusOnPlatform { get; set; }
    public EPrivacyStatus PrivacyStatus { get; set; }
    public DataFetch? LastSuccessfulFetch { get; set; }
    public DataFetch? LastUnSuccessfulFetch { get; set; }
    public DateTimeOffset AddedToArchiveAt { get; set; }

    public ICollection<DataFetch>? DataFetches { get; set; }

    public required AuthorSimple Author { get; set; }

    public ICollection<CommentDto>? ConversationReplies { get; set; }
    public ICollection<CommentDto>? DirectReplies { get; set; }

    public string? Content { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool? AuthorIsCreator { get; set; }

    public TimeSpan? CreatedAtVideoTimecode { get; set; }

    public long OrderIndex { get; set; }

    public CommentStatisticSnapshotDto? Statistics { get; set; }

    public Guid VideoId { get; set; }
}