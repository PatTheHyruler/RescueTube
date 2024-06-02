using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities;

public class AuthorStatisticSnapshot : BaseIdDbEntity
{
    public long? FollowerCount { get; set; }
    public long? PaidFollowerCount { get; set; }

    public DateTimeOffset ValidAt { get; set; }

    public Guid AuthorId { get; set; }
    public Author? Author { get; set; }
}