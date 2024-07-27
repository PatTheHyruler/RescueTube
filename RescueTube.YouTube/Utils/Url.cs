using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace RescueTube.YouTube.Utils;

public static class Url
{
    private static readonly Regex VideoRegex = new(
        @"(?:https?://)?(?:(?:(?:www\.)?(?:youtube\.com)/(?:watch\?.*v=(?<id>(?:(?![&=\?])[\S]){11})))|(?:shorts/(?<id>(?:(?![&=\?])[\S]){11}))|(?:youtu\.be/(?<id>(?:(?![&=\?])[\S]){11})))");

    private static readonly Regex PlaylistRegex = new(
        @"(?:https?://)?(?:www\.)?youtube\.com/(?:(?:playlist\?list=(?<id>(?:(?![&=\?])[\S])+))|(?:watch.*list=(?<id>(?:(?![&=\?])[\S])+)))");

    private static readonly Regex AuthorHandleRegex = new(
        @"(?:https?://)?(?:www\.)youtube\.com/(?<handle>@(?:(?![&=\?/])[\S])+)");

    private static readonly Regex VideoThumbnailRegex = new(
        @"(?:https?://)?(i\.ytimg\.com/vi(?:_webp)?)/(?<id>(?:(?![&=\?])[\S]){11})/(?<quality>maxres|sd|hq|mq)?(?<tag>default|1|2|3|(?:\w+))\.(?<ext>\w+)");

    private static bool IsRegexUrl(string url, Regex regex, string valueGroupName, out string? value)
    {
        value = null;
        var match = regex.Match(url);
        if (!match.Success) return false;
        if (match.Groups.TryGetValue(valueGroupName, out var valueGroup))
        {
            if (valueGroup.Success)
            {
                value = valueGroup.Value;
                return true;
            }
        }

        return false;
    }

    public static bool IsVideoUrl(string url, [NotNullWhen(true)] out string? id) =>
        IsRegexUrl(url, VideoRegex, "id", out id);

    public static bool IsPlaylistUrl(string url, [NotNullWhen(true)] out string? id) =>
        IsRegexUrl(url, PlaylistRegex, "id", out id);

    public static bool IsAuthorHandleUrl(string url, [NotNullWhen(true)] out string? handle) =>
        IsRegexUrl(url, AuthorHandleRegex, "handle", out handle);

    public static bool IsVideoThumbnailUrl(string url, out ParsedThumbnailInfo parsedThumbnailInfo)
    {
        parsedThumbnailInfo = new ParsedThumbnailInfo();

        var match = VideoThumbnailRegex.Match(url);
        if (!match.Success) return false;

        if (match.Groups.TryGetValue("quality", out var valueGroup) && valueGroup.Success)
        {
            parsedThumbnailInfo.Quality = ThumbnailQuality.FromString(valueGroup.Value);
        }

        if (match.Groups.TryGetValue("tag", out valueGroup) && valueGroup.Success)
        {
            parsedThumbnailInfo.Tag = ThumbnailTag.FromString(valueGroup.Value);
        }

        if (match.Groups.TryGetValue("ext", out valueGroup) && valueGroup.Success)
        {
            parsedThumbnailInfo.Ext = valueGroup.Value;
        }

        return true;
    }

    public static string ToVideoUrl(string id)
    {
        return $"https://www.youtube.com/watch?v={id}";
    }

    public static string ToAuthorUrl(string id)
    {
        return $"https://www.youtube.com/channel/{id}";
    }

    public static string ToVideoEmbedUrl(string id)
    {
        return $"https://www.youtube-nocookie.com/embed/{id}";
    }

    public static string ToPlaylistUrl(string id)
    {
        return $"https://www.youtube.com/playlist?list={id}";
    }
}