using RescueTube.Domain.Base;
using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Entities;

public class DataFetch : BaseIdDbEntity
{
    public required DateTimeOffset OccurredAt { get; set; }
    public required DataFetchStatus Status { get; set; }
    public required string Type { get; set; }
    public required string Source { get; set; }
    public string? Message { get; set; }

    public required EPlatform Platform { get; init; }

    public string? VideoIdOnPlatform { get; set; }
    public string? AuthorIdOnPlatform { get; set; }
    public string? PlaylistIdOnPlatform { get; set; }

    public ICollection<DataFetchResult> DataFetchResults { get; set; } = [];
}