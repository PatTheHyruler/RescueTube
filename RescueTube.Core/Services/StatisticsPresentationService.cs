using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Services;

public class StatisticsPresentationService
{
    private readonly AppDbContext _dbCtx;

    public StatisticsPresentationService(AppDbContext dbCtx)
    {
        _dbCtx = dbCtx;
    }

    public async Task<List<VideoDownloadStatisticByPlatformDto>> GetVideoDownloadStatisticsAsync()
    {
        var query = _dbCtx.Videos
            .Select(v => new
            {
                v.Platform,
                HasVideoFile = v.VideoFiles!.Any()
            })
            .GroupBy(x => new { x.Platform, x.HasVideoFile })
            .Select(g => new VideoDownloadStatisticByPlatformDto(
                g.Key.Platform, g.Key.HasVideoFile, g.Count()
            ));
        return await query.ToListAsync();
    }

    public record VideoDownloadStatisticByPlatformDto(EPlatform Platform, bool HasVideoFile, int Count);
}