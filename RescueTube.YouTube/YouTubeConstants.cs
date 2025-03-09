using RescueTube.Domain;
using RescueTube.Domain.Enums;

namespace RescueTube.YouTube;

public static class YouTubeConstants
{
    public static class IdTypes
    {
        public static class Author
        {
            public const string Handle = "handle";
        }
    }

    public static class DataFetches
    {
        public static class YtDlp
        {
            public static readonly DataFetchDefinition VideoPage = new()
            {
                Type = FetchTypes.YtDlp.VideoPage,
                Source = FetchTypes.YtDlp.Source,
                EntityType = EEntityType.Video,
                Platform = EPlatform.YouTube,
            };

            public static readonly DataFetchDefinition VideoFileDownload = new()
            {
                Source = FetchTypes.YtDlp.Source,
                Type = FetchTypes.YtDlp.VideoFileDownload,
                Platform = EPlatform.YouTube,
                EntityType = EEntityType.Video,
            };
        }
    }

    public static class FetchTypes
    {
        public static class YtDlp
        {
            public const string Source = "yt-dlp";

            public const string ChannelVideos = "channelvideos";
            public const string VideoPage = "videopage";
            public const string Playlist = "playlist";
            public const string Comments = "comments";
            public const string VideoFileDownload = Core.Constants.DataFetches.DataFetchTypes.VideoFileDownload;
        }

        public static class YouTubeExplode
        {
            public const string Source = "ytexplode";

            public const string Channel = "channel";
        }

        public static class General
        {
            public const string Source = "general";

            public const string VideoAuthor = "videoauthor";
        }
    }
}