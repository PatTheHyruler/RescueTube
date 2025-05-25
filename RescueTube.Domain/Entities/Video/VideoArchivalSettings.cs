namespace RescueTube.Domain.Entities;

public class VideoArchivalSettings
{
    public bool ShouldRegularlyFetchVideoData { get; set; } = true;

    public static VideoArchivalSettings CreateDefaultArchivedVideoSettings()
    {
        return new()
        {
            ShouldRegularlyFetchVideoData = true,
        };
    }
}