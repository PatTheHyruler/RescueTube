using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RescueTube.Domain.Enums;

namespace WebApp.ApiControllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class OptionsController : ControllerBase
{
    [HttpGet]
    public ICollection<EPlatform> SupportedPlatforms()
    {
        return Enum.GetValues<EPlatform>();
    }
}