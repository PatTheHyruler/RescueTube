using RescueTube.Domain.Base;
using RescueTube.Domain.Contracts;

namespace RescueTube.Domain.Entities;

public class AuthorHistory : BaseIdDbEntity, IHistoryEntity<Author>
{
    public Guid CurrentId { get; set; }
    public Author? Current { get; set; }
    public DateTimeOffset? LastOfficialValidAt { get; set; }
    public DateTimeOffset LastValidAt { get; set; }
    public DateTimeOffset FirstNotValidAt { get; set; }

    public string? UserName { get; set; }
    public string? DisplayName { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}