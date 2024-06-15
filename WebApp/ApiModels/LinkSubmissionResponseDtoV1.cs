using RescueTube.Domain.Enums;

namespace WebApp.ApiModels;

public class LinkSubmissionResponseDtoV1
{
    public required Guid SubmissionId { get; set; }
    public required EEntityType Type { get; set; }
    public required EPlatform Platform { get; set; }
    public required string IdOnPlatform { get; set; }
}