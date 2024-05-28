using RescueTube.Domain.Base;
using RescueTube.Domain.Contracts;

namespace RescueTube.Domain.Entities;

public class CommentHistory : BaseIdDbEntity, IHistoryEntity<Comment>
{
    public Guid CurrentId { get; set; }
    public Comment? Current { get; set; }
    
    public DateTimeOffset? LastOfficialValidAt { get; set; }
    public DateTimeOffset LastValidAt { get; set; }
    public DateTimeOffset FirstNotValidAt { get; set; }

    public string? Content { get; set; }
    public TimeSpan? CreatedAtVideoTimecode { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}