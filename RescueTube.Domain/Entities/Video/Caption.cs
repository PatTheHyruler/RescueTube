using RescueTube.Domain.Base;
using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Entities;

public class Caption : BaseIdDbEntity
{
    public EPlatform Platform { get; set; }
    public string? IdOnPlatform { get; set; }

    public string? Culture { get; set; }
    public required string Ext { get; set; }
    public string? FilePath { get; set; }
    public string? Url { get; set; }
    public string? Name { get; set; }
    public string? Etag { get; set; }

    public DateTimeOffset? ValidSince { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public DateTimeOffset? LastFetched { get; set; }

    public Guid VideoId { get; set; }
    public Video? Video { get; set; }
}