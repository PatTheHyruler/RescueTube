using RescueTube.Domain.Enums;

namespace WebApp.ApiModels.Statistics;

public class VideoDownloadStatisticByPlatformDtoV1
{
    public required EPlatform Platform { get; set; }
    public required bool HasVideoFile { get; set; }
    public required int Count { get; set; }
}