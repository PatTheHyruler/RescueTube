using BLL.DTO.Enums;
using DAL.EF.DbContexts;
using DAL.EF.Pagination;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Utils.Pagination.Contracts;

namespace DAL.EF.Extensions;

public static class VideoExtensions
{
    private static IQueryable<Video>
        FilterByIdOnPlatform(this IQueryable<Video> query, string id, EPlatform platform) =>
        query.Where(e => e.Platform == platform && e.IdOnPlatform == id);

    public static Task<Video?> GetByIdOnPlatformAsync(this IQueryable<Video> query, string id, EPlatform platform) =>
        query.FilterByIdOnPlatform(id, platform)
            .FirstOrDefaultAsync();

    public static IQueryable<Video> SearchVideos(
        this IQueryable<Video> query, AbstractAppDbContext dbContext,
        EPlatform? platform, string? name, string? author,
        ICollection<Guid>? categoryIds,
        Guid? userId, Guid? userAuthorId, bool accessAllowed,
        IPaginationQuery paginationQuery, EVideoSortingOptions sortingOptions,
        bool descending)
    {
        if (platform != null)
        {
            query = query.Where(e => e.Platform == platform);
        }
        
        if (name != null)
        {
            var nameQuery = "%" + Utils.EscapeWildcards(name) + "%";
            query = query.Where(e => e.Title!.Translations!
                .Any(t => Microsoft.EntityFrameworkCore.EF.Functions
                    .ILike(t.Content, nameQuery, "\\")));
            // TODO: SQLI?
        }

        if (author != null)
        {
            var authorQuery = "%" + Utils.EscapeWildcards(author) + "%";
            query = query.Where(e => e.VideoAuthors!
                .Select(a => a.Author!.UserName + a.Author!.DisplayName)
                .Any(n => Microsoft.EntityFrameworkCore.EF.Functions.ILike(n, authorQuery)));
        }

        if (categoryIds is { Count: > 0 })
        {
            // TODO: Categories
        }

        if (!accessAllowed)
        {
            query = query.WhereUserIsAllowedToAccessVideoOrVideoIsPublic(
                dbContext, userId
            );
        }

        switch (sortingOptions)
        {
            case EVideoSortingOptions.Duration:
                query = descending
                    ? query.OrderByDescending(v => v.Duration)
                    : query.OrderBy(v => v.Duration);
                break;
            case EVideoSortingOptions.CreatedAt:
                query = descending
                    ? query.OrderByDescending(v => v.PublishedAt).ThenByDescending(v => v.CreatedAt)
                    : query.OrderBy(v => v.PublishedAt).ThenByDescending(v => v.CreatedAt);
                break;
        }

        query = query.Paginate(paginationQuery);

        return query;
    }

    public static IQueryable<Video> WhereUserIsAllowedToAccessVideoOrVideoIsPublic(
        this IQueryable<Video> query, AbstractAppDbContext dbContext, Guid? userId)
    {
        if (userId != null)
        {
            return query.Where(v => v.PrivacyStatus == EPrivacyStatus.Public ||
                                    dbContext.EntityAccessPermissions.Any(p =>
                                        p.VideoId == v.Id && p.UserId == userId)
                                    // TODO: Playlists
                                    // || dbContext.PlaylistVideos.Any(p => p.VideoId == v.Id &&
                                    //           dbContext.EntityAccessPermissions.Any(e =>
                                    //               e.UserId == userId &&
                                    //               e.PlaylistId == p.PlaylistId))
            );
        }

        return query.Where(v => v.PrivacyStatus == EPrivacyStatus.Public);
    }
}