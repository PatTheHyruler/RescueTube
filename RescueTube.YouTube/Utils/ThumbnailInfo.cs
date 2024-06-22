namespace RescueTube.YouTube.Utils;

public record ParsedThumbnailInfo
{
    public ThumbnailQuality? Quality { get; set; }
    public ThumbnailTag? Tag { get; set; }
    public string? Ext { get; set; }
}