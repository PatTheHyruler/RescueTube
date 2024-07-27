using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities;

public class DataFetch : BaseIdDbEntity
{
    public required DateTimeOffset OccurredAt { get; set; }
    public required bool Success { get; set; }
    public required string Type { get; set; }
    public required bool ShouldAffectValidity { get; set; }
    public required string Source { get; set; }

    public Guid? VideoId { get; set; }
    public Video? Video { get; set; }

    public Guid? AuthorId { get; set; }
    public Author? Author { get; set; }

    public Guid? CommentId { get; set; }
    public Comment? Comment { get; set; }

    public Guid? PlaylistId { get; set; }
    public Playlist? Playlist { get; set; }
}