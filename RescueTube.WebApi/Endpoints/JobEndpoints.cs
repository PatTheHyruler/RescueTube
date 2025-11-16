using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.Identity;
using RescueTube.Core.Services;
using RescueTube.Domain.Entities;
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

        jobsGroup.MapPut("settings", UpdateJobSettingsAsync)
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

    private static async Task UpdateJobSettingsAsync(
        [FromBody] JobSettingsUpdateDtoV1[] jobSettingsUpdates,
        [FromServices] IRecurringJobsService recurringJobsService,
        [FromServices] IDataUow dataUow,
        CancellationToken ct)
    {
        var jobDefinitions = await recurringJobsService.GetJobDefinitionsWithSettingsAsync(ct);

        var joinedUpdates = jobSettingsUpdates.Join(
                jobDefinitions,
                x => x.JobId,
                x => x.JobDefinition.JobId,
                (updateDto, x) => (JobDefinitionWithSettings: x, UpdateDto: updateDto))
            .ToArray();

        foreach (var ((_, currentSettings), updateDto) in joinedUpdates)
        {
            if (currentSettings is not PersistedJobSettings persistedJobSettings)
            {
                persistedJobSettings = currentSettings.CloneToPersistedJobSettings();
                dataUow.Ctx.JobSettings.Add(persistedJobSettings);
            }
            else
            {
                dataUow.Ctx.Entry(persistedJobSettings).State = EntityState.Unchanged;
            }

            persistedJobSettings.IsEnabled = updateDto.IsEnabled;
            persistedJobSettings.Cron = updateDto.Cron;

            if (persistedJobSettings.DataFetchJobSettings is not null && updateDto.DataFetchJobSettings is not null)
            {
                persistedJobSettings.DataFetchJobSettings.SuccessCutoffOffset = updateDto.DataFetchJobSettings.SuccessCutoffOffset;
                persistedJobSettings.DataFetchJobSettings.FailureCutoffOffset = updateDto.DataFetchJobSettings.FailureCutoffOffset;
            }
        }

        await dataUow.SaveChangesAsync(ct);

        await recurringJobsService.HandleJobSettingsUpdateAsync(
            joinedUpdates.Select(x => x.JobDefinitionWithSettings).ToArray(),
            ct);
    }
}