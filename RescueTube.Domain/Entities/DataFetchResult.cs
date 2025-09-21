using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities;

public class DataFetchResult : BaseIdDbEntity
{
    public Guid DataFetchId { get; init; }
    public DataFetch? DataFetch { get; init; }

    public Guid? VideoId { get; init; }
    public Video? Video { get; init; }

    public Guid? AuthorId { get; init; }
    public Author? Author { get; init; }

    public Guid? PlaylistId { get; init; }
    public Playlist? Playlist { get; init; }

    public Guid? CommentId { get; init; }
    public Comment? Comment { get; init; }
}