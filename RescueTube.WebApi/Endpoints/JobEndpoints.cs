using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.DTO;
using RescueTube.Core.Identity;
using RescueTube.Core.Services;
using RescueTube.Domain.Entities;
using RescueTube.WebApi.ApiModels;
using RescueTube.WebApi.ApiModels.Mappers;
using RescueTube.WebApi.Utils.Validation;

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

    private static async Task<Results<Ok, BadRequest<ErrorResponseDto>>> UpdateJobSettingsAsync(
        [FromBody] JobSettingsUpdateDtoV1[] jobSettingsUpdates,
        [FromServices] IRecurringJobsService recurringJobsService,
        [FromServices] IDataUow dataUow,
        CancellationToken ct)
    {
        var validator = new JobSettingsUpdateDtoV1CollectionValidator();
        var validationResult = await validator.ValidateAsync(jobSettingsUpdates, ct);
        if (!validationResult.IsValid(out var badRequestResponse))
        {
            return badRequestResponse;
        }

        var jobDefinitions = await recurringJobsService.GetJobDefinitionsWithSettingsAsync(ct);

        var joinedUpdates = jobSettingsUpdates.Join(
                jobDefinitions,
                x => x.JobId,
                x => x.JobDefinition.JobId,
                (updateDto, x) => (JobDefinitionWithSettings: x, UpdateDto: updateDto))
            .ToArray();

        var updatedDefinitionsWithSettings = new List<JobDefinitionWithSettings>(joinedUpdates.Length);

        foreach (var (definitionWithSettings, updateDto) in joinedUpdates)
        {
            if (definitionWithSettings.JobSettings is not PersistedJobSettings persistedJobSettings)
            {
                persistedJobSettings = definitionWithSettings.JobSettings.CloneToPersistedJobSettings();
                dataUow.Ctx.JobSettings.Add(persistedJobSettings);
            }
            else
            {
                dataUow.Ctx.Entry(persistedJobSettings).State = EntityState.Unchanged;
            }

            updatedDefinitionsWithSettings.Add(definitionWithSettings with
            {
                JobSettings = persistedJobSettings,
            });

            persistedJobSettings.IsEnabled = updateDto.IsEnabled;
            persistedJobSettings.Cron = updateDto.Cron;

            if (persistedJobSettings.DataFetchJobSettings is not null && updateDto.DataFetchJobSettings is not null)
            {
                persistedJobSettings.DataFetchJobSettings.SuccessCutoffOffset = updateDto.DataFetchJobSettings.SuccessCutoffOffset;
                persistedJobSettings.DataFetchJobSettings.FailureCutoffOffset = updateDto.DataFetchJobSettings.FailureCutoffOffset;
            }
        }

        await dataUow.SaveChangesAsync(ct);

        await recurringJobsService.HandleJobSettingsUpdateAsync(updatedDefinitionsWithSettings, ct);

        return TypedResults.Ok();
    }
}