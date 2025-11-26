using RescueTube.Domain.Base;
using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Entities;

public class DataFetch : BaseIdDbEntity
{
    public required DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? StatusUpdatedAt { get; set; }
    public required DataFetchStatus Status { get; set; }
    public required string Type { get; set; }
    public required string Source { get; set; }
    public string? Message { get; set; }

    public Guid? VideoId { get; set; }
    public Video? Video { get; set; }

    public Guid? AuthorId { get; set; }
    public Author? Author { get; set; }

    public Guid? PlaylistId { get; set; }
    public Playlist? Playlist { get; set; }

    public Guid? SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    public required EPlatform Platform { get; init; }

    public DateTimeOffset? LastHeartbeatReceivedAt { get; set; }

    public ICollection<DataFetchResult> DataFetchResults { get; set; } = [];
}