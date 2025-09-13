using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data;
using RescueTube.Core.Identity;
using RescueTube.Core.Services;
using RescueTube.Domain.Entities;
using RescueTube.WebApi.ApiModels;
using RescueTube.WebApi.ApiModels.Mappers;
using RescueTube.WebApi.Utils;

namespace RescueTube.WebApi.Endpoints;

public static class AuthorEndpoints
{
    public static void MapAuthorEndpoints(this IEndpointRouteBuilder app)
    {
        var authorsGroup = app.MapGroup("authors").WithTags("Authors");

        authorsGroup.MapGet("{authorId:guid}", GetAuthorAsync).HasApiVersion(1);

        authorsGroup.MapGet("", GetAuthorsAsync).HasApiVersion(1);

        authorsGroup.MapGet("{authorId:guid}/archival-settings", GetAuthorArchivalSettingsAsync)
            .HasApiVersion(1);

        authorsGroup.MapPut("{authorId:guid}/archival-settings", UpsertAuthorArchivalSettingsAsync)
            .RequireAuthorization(p => p.RequireRole(RoleNames.AdminRoles))
            .HasApiVersion(1);
    }

    private static async Task<Results<Ok<AuthorSimpleDtoV1>, NotFound<ErrorResponseDto>>> GetAuthorAsync(
        [FromRoute] Guid authorId, HttpContext httpContext,
        [FromServices] AuthorPresentationService authorPresentationService, CancellationToken ct)
    {
        var author = await authorPresentationService.GetAuthorSimpleAsync(authorId, ct);
        if (author is null)
        {
            return TypedResults.NotFound(new ErrorResponseDto
            {
                ErrorType = EErrorType.EntityNotFound,
                Message = "Author not found",
            });
        }

        return TypedResults.Ok(author.MapAuthorSimpleDtoV1(httpContext.GetBaseUrl()));
    }

    private static async Task<Ok<AuthorSearchResponseDtoV1>> GetAuthorsAsync(
        [AsParameters] AuthorSearchDtoV1 request,
        [FromServices] AuthorPresentationService authorPresentationService,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var result = await authorPresentationService.SearchAuthorsSimpleAsync(
            request,
            name: request.Name,
            authorIds: request.AuthorIds,
            excludeAuthorIds: request.ExcludeAuthorIds,
            ct);
        return TypedResults.Ok(new AuthorSearchResponseDtoV1
        {
            Authors = result.Result
                .Select(a => a.MapAuthorSimpleDtoV1(httpContext.GetBaseUrl()))
                .ToArray(),
            Page = result.PaginationResult.Page,
            AmountOnPage = result.PaginationResult.AmountOnPage,
            TotalResults = result.PaginationResult.TotalResults,
            Limit = result.PaginationResult.Limit,
        });
    }

    private static async Task<Ok<AuthorArchivalSettingsDtoV1?>> GetAuthorArchivalSettingsAsync(
        [FromRoute] Guid authorId, [FromServices] IDataUow dataUow, CancellationToken ct)
    {
        var authorSettings = await dataUow.Ctx.Authors
            .Where(a => a.Id == authorId && a.ArchivalSettingsId != null)
            .Select(a => a.ArchivalSettings)
            .FirstOrDefaultAsync(ct);

        if (authorSettings is null)
        {
            return TypedResults.Ok<AuthorArchivalSettingsDtoV1?>(null);
        }

        return TypedResults.Ok<AuthorArchivalSettingsDtoV1?>(
            authorSettings.MapToAuthorArchivalSettingsDtoV1(authorId: authorId));
    }

    private static async Task<Results<Ok<AuthorArchivalSettingsDtoV1>, NotFound<ErrorResponseDto>>> UpsertAuthorArchivalSettingsAsync(
        [FromRoute] Guid authorId, [FromBody] AuthorArchivalSettingsUpsertDtoV1 settingsDto,
        [FromServices] IDataUow dataUow, CancellationToken ct)
    {
        var author = await dataUow.Ctx.Authors
            .Include(a => a.ArchivalSettings)
            .FirstOrDefaultAsync(a => a.Id == authorId, ct);
        if (author is null)
        {
            return TypedResults.NotFound(new ErrorResponseDto
            {
                ErrorType = EErrorType.EntityNotFound,
                Message = "Author not found",
            });
        }

        if (author.ArchivalSettings is null)
        {
            author.ArchivalSettings = new AuthorArchivalSettings();
            dataUow.Ctx.AuthorArchivalSettings.Add(author.ArchivalSettings);
        }

        author.ArchivalSettings.IsEnabledForArchival = settingsDto.IsEnabledForArchival;
        author.ArchivalSettings.ArchiveVideos = settingsDto.ArchiveVideos;
        author.ArchivalSettings.ArchiveClips = settingsDto.ArchiveClips;
        author.ArchivalSettings.ArchivePlaylists = settingsDto.ArchivePlaylists;

        await dataUow.SaveChangesAsync(ct);

        return TypedResults.Ok(author.ArchivalSettings.MapToAuthorArchivalSettingsDtoV1(authorId: authorId));
    }
}