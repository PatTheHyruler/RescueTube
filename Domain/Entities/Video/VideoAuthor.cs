using Base.Domain;
using Domain.Enums;

namespace Domain.Entities;

public class VideoAuthor : AbstractIdDatabaseEntity
{
    public Guid VideoId { get; set; }
    public Video? Video { get; set; }

    public Guid AuthorId { get; set; }
    public Author? Author { get; set; }

    public EAuthorRole Role { get; set; } = EAuthorRole.Publisher;
}