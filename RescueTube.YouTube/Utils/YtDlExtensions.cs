using YoutubeDLSharp;

namespace RescueTube.YouTube.Utils;

public static class YtDlExtensions
{
    public static string? ErrorOutputToString<T>(this RunResult<T>? runResult) =>
        runResult?.ErrorOutput?.Aggregate("", (a, b) => $"'{a}', '{b}'");
}