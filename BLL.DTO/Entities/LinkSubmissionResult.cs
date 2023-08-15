using Domain.Enums;

namespace BLL.DTO.Entities;

public class LinkSubmissionResult
{
    public Guid SubmissionId { get; set; }
    public EEntityType Type { get; set; }
    public Guid? EntityId { get; set; }
    public EPlatform Platform { get; set; }
    public string IdOnPlatform { get; set; } = default!;
    public bool AlreadyAdded { get; set; }
}