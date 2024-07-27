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

    public static class FetchTypes
    {
        public static class YtDlp
        {
            public const string Source = "yt-dlp";

            public const string ChannelPage = "channelpage";
            public const string VideoPage = "videopage";
            public const string Playlist = "playlist";
            public const string Comments = "comments";
            public const string VideoFileDownload = "videofiledownload";
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