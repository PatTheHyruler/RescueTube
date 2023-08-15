using Base.Domain;
using Domain.Entities.Identity;
using Domain.Enums;

namespace Domain.Entities;

public class Submission : AbstractIdDatabaseEntity
{
    public EPlatform Platform { get; set; }
    public string IdOnPlatform { get; set; } = default!;
    public EEntityType EntityType { get; set; }

    public Guid AddedById { get; set; }
    public User? AddedBy { get; set; }
    public DateTime AddedAt { get; set; }

    public Guid? ApprovedById { get; set; }
    public User? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public bool GrantAccess { get; set; } = true;

    public DateTime? CompletedAt { get; set; }

    public Guid? VideoId { get; set; }
    public Video? Video { get; set; }
}