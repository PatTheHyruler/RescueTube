namespace RescueTube.Domain.Entities;

public class VideoArchivalSettings
{
    public bool ShouldRegularlyFetchVideoData { get; set; } = true;
    public int DownloadPriority { get; set; }

    public static VideoArchivalSettings CreateDefaultArchivedVideoSettings()
    {
        return new()
        {
            ShouldRegularlyFetchVideoData = true,
            DownloadPriority = 0,
        };
    }
}