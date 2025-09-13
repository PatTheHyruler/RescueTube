using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RescueTube.Core.Services;
using RescueTube.WebApi.ApiModels.Mappers;
using RescueTube.WebApi.ApiModels.Statistics;

namespace RescueTube.WebApi.Endpoints;

public static class StatisticsEndpoints
{
    public static void MapStatisticsEndpoints(this IEndpointRouteBuilder app)
    {
        var optionsGroup = app.MapGroup("statistics").WithTags("Statistics");

        optionsGroup.MapGet("VideoDownloadStatistics", GetVideoDownloadStatisticsAsync).HasApiVersion(1);
    }

    private static async Task<Ok<VideoDownloadStatisticsByPlatformResponseDtoV1>> GetVideoDownloadStatisticsAsync(
        [FromServices] StatisticsPresentationService statisticsPresentationService, CancellationToken ct)
    {
        var statistics = await statisticsPresentationService.GetVideoDownloadStatisticsAsync(ct);
        return TypedResults.Ok(new VideoDownloadStatisticsByPlatformResponseDtoV1
        {
            VideoDownloadStatistics = statistics.Select(ApiMapper.MapVideoDownloadStatisticByPlatformDtoV1).ToList(),
        });
    }
}