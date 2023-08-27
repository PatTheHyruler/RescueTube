namespace BLL.YouTube;

public class YouTubeOptions
{
    public const string Section = "YouTube";

    public string? BinariesDirectory { get; set; }
    public bool OverwriteExistingBinaries { get; set; } = false;
}