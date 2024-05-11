using Domain.Base;

namespace Domain.Entities;

public class VideoCategory : BaseIdDbEntity
{
    public Guid VideoId { get; set; }
    public Video? Video { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public Guid? AssignedById { get; set; }
    public Author? AssignedBy { get; set; }
}