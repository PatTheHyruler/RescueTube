using Base.Domain;

namespace Domain.Entities;

public class VideoCategory : AbstractIdDatabaseEntity
{
    public Guid VideoId { get; set; }
    public Video? Video { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public Guid? AssignedById { get; set; }
    public Author? AssignedBy { get; set; }
}