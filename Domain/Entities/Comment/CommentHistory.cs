using Domain.Base;
using Domain.Contracts;

namespace Domain.Entities;

public class CommentHistory : BaseIdDbEntity, IHistoryEntity<Comment>
{
    public Guid CurrentId { get; set; }
    public Comment? Current { get; set; }
    
    public DateTime? LastOfficialValidAt { get; set; }
    public DateTime LastValidAt { get; set; }
    public DateTime FirstNotValidAt { get; set; }

    public string? Content { get; set; }
    public TimeSpan? CreatedAtVideoTimecode { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}