using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RescueTube.Core.Data;
using RescueTube.Core.Errors;
using RescueTube.Core.Identity;
using RescueTube.Core.Services;
using RescueTube.Domain;
using RescueTube.Domain.Entities;
using WebApp.ApiModels;
using WebApp.ApiModels.Mappers;
using WebApp.ApiModels.Settings;

namespace WebApp.Endpoints;

public static class SettingEndpoints
{
    public static void MapSettingEndpoints(this IEndpointRouteBuilder app)
    {
        var settingsGroup = app.MapGroup("settings").WithTags("Settings");

        settingsGroup.MapGet("", GetSettingsAsync)
            .RequireAuthorization(p => p.RequireRole(RoleNames.SuperAdmin))
            .HasApiVersion(1);

        settingsGroup.MapPut("", UpsertSettingAsync)
            .RequireAuthorization(p => p.RequireRole(RoleNames.SuperAdmin))
            .HasApiVersion(1);
    }

    private static async Task<Ok<SettingValueDtoV1[]>> GetSettingsAsync(
        [FromServices] SettingService settingService, CancellationToken ct)
    {
        var settings = await settingService.GetGeneralSettingsAsync(ct);
        var mappedSettings = settings
            .Select(SettingMapper.MapToSettingValueDtoV1)
            .ToArray();
        return TypedResults.Ok(mappedSettings);
    }

    private static async Task<Results<Ok, BadRequest<ErrorResponseDto>>> UpsertSettingAsync(
        [FromBody] SettingValueUpdateDtoV1 settingValueUpdateDto,
        [FromServices] SettingService settingService, [FromServices] IDataUow dataUow, CancellationToken ct)
    {
        RescueTube.Core.Utils.IResult<Setting, SettingDefinitionNotFoundError> result = settingValueUpdateDto switch
        {
            SettingValueUpdateDtoV1.Long v => await settingService.SetValueAsync(v.Key, v.Value, ct),
            SettingValueUpdateDtoV1.Bool v => await settingService.SetValueAsync(v.Key, v.Value, ct),
            SettingValueUpdateDtoV1.String v => await settingService.SetValueAsync(v.Key, v.Value, ct),
            SettingValueUpdateDtoV1.DataSizeBytes v => await settingService.SetValueAsync(v.Key, DataSize.FromBytes(v.Value), ct),
            _ => throw new ArgumentOutOfRangeException(nameof(settingValueUpdateDto), settingValueUpdateDto, @"Unrecognized setting value type"),
        };
        if (!result.Success)
        {
            return TypedResults.BadRequest(new ErrorResponseDto
            {
                ErrorType = EErrorType.SettingKeyNotFound,
                Message = $"Setting with key '{result.Error.Key}' and type '{result.Error.RequestedType?.Name}' not found",
            });
        }

        await dataUow.SaveChangesAsync(ct);
        return TypedResults.Ok();
    }
}