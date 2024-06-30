using RescueTube.Core.Contracts;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Utils;

namespace RescueTube.YouTube.Services;

public class ThumbnailComparer : IThumbnailComparer
{
    public string?[] Qualities => ThumbnailQuality.AllQualityNames.ToArray();
    public string?[] Keys => ThumbnailTag.AllTags.Reverse().Select(t => t.Identifier).ToArray();
    public EPlatform Platform => EPlatform.YouTube;
}