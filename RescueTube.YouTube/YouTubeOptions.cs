namespace RescueTube.YouTube;

public class YouTubeOptions
{
    public const string Section = "YouTube";

    public string? BinariesDirectory { get; set; }
    public string CookiesDirectory { get; set; } = "config/youtube/cookies";
    public bool OverwriteExistingBinaries { get; set; } = false;
}