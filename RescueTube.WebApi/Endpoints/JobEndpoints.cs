using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RescueTube.Core.JobOrchestration;

namespace RescueTube.WebApi.Endpoints;

public static class JobEndpoints
{
    public static void MapJobEndpoints(this IEndpointRouteBuilder app)
    {
        var jobsGroup = app.MapGroup("jobs").WithTags("Jobs");

        jobsGroup.MapGet("stats", GetJobStats).HasApiVersion(1);
    }

    /// <summary>
    /// Temporary debug endpoint
    /// </summary>
    /// <returns>Some kind of object</returns>
    private static Ok<object> GetJobStats(
        [FromServices] JobExecutionRegistry jobExecutionRegistry,
        [FromServices] IOptions<JobsConfiguration> jobsConfig)
    {
        return TypedResults.Ok<object>(new
        {
            LatestJobs = jobExecutionRegistry.StartedJobs
                .SelectMany(x => x.Value.Values
                    .Select(info => new { Job = x.Key.Name, InvocationInfo = info }))
                .OrderByDescending(x => x.InvocationInfo.StartedAt)
                .Take(15),
            LatestJobsByType = jobExecutionRegistry.StartedJobs.Select(kvp => new
            {
                Job = kvp.Key.Name,
                Latest = kvp.Value.Values.OrderByDescending(x => x.StartedAt).FirstOrDefault(),
            }),
            JobsWithPriority = jobsConfig.Value.RegisteredJobs.Select(j => new
            {
                Job = j.Name,
                Priority = jobExecutionRegistry.GetJobPriority(j),
            }),
        });
    }
}