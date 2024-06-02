using RescueTube.Domain.Base;
using RescueTube.Domain.Entities.Identity;

namespace RescueTube.Domain.Entities;

public class EntityAccessPermission : BaseIdDbEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid? VideoId { get; set; }
    public Video? Video { get; set; }
    public Guid? AuthorId { get; set; }
    public Author? Author { get; set; }
}