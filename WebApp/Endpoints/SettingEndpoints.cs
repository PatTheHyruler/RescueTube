using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RescueTube.Core.Data;
using RescueTube.Core.Identity;
using RescueTube.Core.Services;
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

        settingsGroup.MapPut("bulk", UpsertSettingsAsync)
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

    private static async Task UpsertSettingsAsync(
        [FromBody] IEnumerable<SettingValueUpdateDtoV1> settingValueUpdates,
        [FromServices] SettingService settingService, [FromServices] IDataUow dataUow, CancellationToken ct)
    {
        var mappedSettingValueUpdates = settingValueUpdates.Select(SettingMapper.MapToCoreSettingValueUpdateDto);
        await settingService.UpdateSettingsAsync(mappedSettingValueUpdates, ct);
        await dataUow.SaveChangesAsync(ct);
    }
}