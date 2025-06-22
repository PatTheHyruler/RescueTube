using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RescueTube.Core.Data;
using RescueTube.Core.Identity;
using RescueTube.Core.Services;
using WebApp.ApiModels;
using WebApp.ApiModels.Mappers;
using WebApp.ApiModels.Settings;
using WebApp.ApiModels.Settings.YouTube;

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

        var youtubeGroup = settingsGroup.MapGroup("youtube").WithTags("YouTube");
        youtubeGroup.MapGet("cookie-files", GetCookieFiles)
            .RequireAuthorization(p => p.RequireRole(RoleNames.SuperAdmin))
            .HasApiVersion(1);

        youtubeGroup.MapPut("cookie-files", CreateCookieFileAsync)
            .RequireAuthorization(p => p.RequireRole(RoleNames.SuperAdmin))
            .HasApiVersion(1);

        youtubeGroup.MapDelete("cookie-files", DeleteCookieFile)
            .RequireAuthorization(p => p.RequireRole(RoleNames.SuperAdmin))
            .HasApiVersion(1);

        youtubeGroup.MapPost("cookie-files/rename", RenameCookieFile)
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

    private static Ok<CookieFileInfoDtoV1[]> GetCookieFiles(
        [FromServices] RescueTube.YouTube.Services.CookieService cookieService)
    {
        var files = cookieService.GetCookieFiles()
            .Select(f => new CookieFileInfoDtoV1
            {
                FileName = f.Name,
            })
            .ToArray();
        return TypedResults.Ok(files);
    }

    private static async Task CreateCookieFileAsync(
        [FromBody] CreateCookieFileDtoV1 createCookieFileDto,
        [FromServices] RescueTube.YouTube.Services.CookieService cookieService,
        CancellationToken ct)
    {
        await cookieService.CreateCookieFileAsync(
            content: createCookieFileDto.Content,
            fileName: createCookieFileDto.FileName,
            ct);
    }

    private static Results<Ok, NotFound<ErrorResponseDto>> DeleteCookieFile(
        [FromQuery] string fileName,
        [FromServices] RescueTube.YouTube.Services.CookieService cookieService)
    {
        var result = cookieService.DeleteCookieFile(fileName);
        if (!result)
        {
            return TypedResults.NotFound(new ErrorResponseDto
            {
                ErrorType = EErrorType.FileNotFound,
            });
        }

        return TypedResults.Ok();
    }

    private static Results<Ok, NotFound<ErrorResponseDto>> RenameCookieFile(
        [FromBody] RenameCookieFileDtoV1 renameCookieFileDto,
        [FromServices] RescueTube.YouTube.Services.CookieService cookieService)
    {
        var result = cookieService.RenameCookieFile(
            oldFileName: renameCookieFileDto.OldFileName,
            newFileName: renameCookieFileDto.NewFileName);

        if (!result)
        {
            return TypedResults.NotFound(new ErrorResponseDto
            {
                ErrorType = EErrorType.FileNotFound,
            });
        }

        return TypedResults.Ok();
    }
}