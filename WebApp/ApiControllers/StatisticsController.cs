using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RescueTube.Core.Services;
using WebApp.ApiModels.Mappers;
using WebApp.ApiModels.Statistics;

namespace WebApp.ApiControllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class StatisticsController : ControllerBase
{
    private readonly StatisticsPresentationService _statisticsPresentationService;

    public StatisticsController(StatisticsPresentationService statisticsPresentationService)
    {
        _statisticsPresentationService = statisticsPresentationService;
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<VideoDownloadStatisticsByPlatformResponseDtoV1>> VideoDownloadStatistics()
    {
        var statistics = await _statisticsPresentationService.GetVideoDownloadStatisticsAsync();
        return Ok(new VideoDownloadStatisticsByPlatformResponseDtoV1
        {
            VideoDownloadStatistics = statistics.Select(ApiMapper.MapVideoDownloadStatisticByPlatformDtoV1).ToList(),
        });
    }
}