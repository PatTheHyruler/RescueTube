namespace Public.DTO.v1;

/// <summary>
/// Information about the result of a URL submission to the archive.
/// </summary>
public class LinkSubmissionResult
{
    public Guid SubmissionId { get; set; }
    public EEntityType Type { get; set; }
    public Guid? EntityId { get; set; }
    public EPlatform Platform { get; set; }
    public string IdOnPlatform { get; set; } = default!;
    public bool AlreadyAdded { get; set; }
}