using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RescueTube.Core.Identity;
using RescueTube.Core.Services;
using RescueTube.WebApi.ApiModels;
using RescueTube.WebApi.ApiModels.Mappers;

namespace RescueTube.WebApi.Endpoints;

public static class JobEndpoints
{
    public static void MapJobEndpoints(this IEndpointRouteBuilder app)
    {
        var jobsGroup = app.MapGroup("jobs").WithTags("Jobs");

        jobsGroup.MapGet("settings", GetJobSettingsAsync)
            .RequireAuthorization(p => p.RequireRole(RoleNames.AdminRoles))
            .HasApiVersion(1);
    }

    private static async Task<Ok<JobSettingsDtoV1[]>> GetJobSettingsAsync(
        [FromServices] IRecurringJobsService recurringJobsService,
        CancellationToken ct)
    {
        var jobDefinitions = await recurringJobsService.GetJobDefinitionsWithSettingsAsync(ct);
        var result = jobDefinitions
            .Select(JobSettingMapper.MapToJobSettingsDtoV1)
            .ToArray();

        return TypedResults.Ok(result);
    }
}