using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RescueTube.Core.JobOrchestration;

namespace WebApp.ApiControllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/jobs")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class JobsController : ControllerBase
{
    private readonly JobExecutionRegistry _jobExecutionRegistry;
    private readonly IOptions<JobsConfiguration> _config;

    public JobsController(JobExecutionRegistry jobExecutionRegistry, IOptions<JobsConfiguration> config)
    {
        _jobExecutionRegistry = jobExecutionRegistry;
        _config = config;
    }

    /// <summary>
    /// Temporary debug endpoint
    /// </summary>
    /// <returns>Some kind of object</returns>
    [HttpGet("stats")]
    public object Stats()
    {
        return new
        {
            LatestJobs = _jobExecutionRegistry.StartedJobs
                .SelectMany(x => x.Value.Values
                    .Select(info => new { Job = x.Key.Name, InvocationInfo = info }))
                .OrderByDescending(x => x.InvocationInfo.StartedAt)
                .Take(15),
            LatestJobsByType = _jobExecutionRegistry.StartedJobs.Select(kvp => new
            {
                Job = kvp.Key.Name,
                Latest = kvp.Value.Values.OrderByDescending(x => x.StartedAt).FirstOrDefault(),
            }),
            JobsWithPriority = _config.Value.RegisteredJobs.Select(j => new
            {
                Job = j.Name,
                Priority = _jobExecutionRegistry.GetJobPriority(j),
            }),
        };
    }
}