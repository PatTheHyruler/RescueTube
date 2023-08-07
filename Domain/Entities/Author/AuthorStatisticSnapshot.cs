using Base.Domain;

namespace Domain.Entities;

public class AuthorStatisticSnapshot : AbstractIdDatabaseEntity
{
    public long? FollowerCount { get; set; }
    public long? PaidFollowerCount { get; set; }

    public Guid AuthorId { get; set; }
    public Author? Author { get; set; }
}