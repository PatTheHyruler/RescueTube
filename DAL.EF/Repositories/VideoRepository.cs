using BLL.Data.Extensions;
using BLL.Data.Pagination;
using BLL.Data.Repositories;
using BLL.DTO.Enums;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DAL.EF.Repositories;

public class VideoRepository : BaseRepository, IVideoRepository
{
    public IQueryable<Video> SearchVideos(IVideoRepository.VideoSearchParams search)
    {
        IQueryable<Video> query = Ctx.Videos;
        
        if (search.Platform != null)
        {
            query = query.Where(e => e.Platform == search.Platform);
        }
        
        if (!string.IsNullOrEmpty(search.Name))
        {
            var nameQuery = "%" + Utils.EscapeWildcards(search.Name) + "%";
            query = query.Where(e => e.Title!.Translations!
                .Any(t => Microsoft.EntityFrameworkCore.EF.Functions
                    .ILike(t.Content, nameQuery, "\\")));
            // TODO: SQLI?
        }

        if (!string.IsNullOrEmpty(search.Author))
        {
            var authorQuery = "%" + Utils.EscapeWildcards(search.Author) + "%";
            query = query.Where(e => e.VideoAuthors!
                .Select(a => a.Author!.UserName + a.Author!.DisplayName)
                .Any(n => Microsoft.EntityFrameworkCore.EF.Functions.ILike(n, authorQuery)));
        }

        if (search.CategoryIds is { Count: > 0 })
        {
            // TODO: Categories
        }

        if (!search.AccessAllowed)
        {
            query = WhereUserIsAllowedToAccessVideoOrVideoIsPublic(query, search.UserId);
        }

        switch (search.SortingOptions)
        {
            case EVideoSortingOptions.Duration:
                query = search.Descending
                    ? query.OrderByDescending(v => v.Duration)
                    : query.OrderBy(v => v.Duration);
                break;
            case EVideoSortingOptions.CreatedAt:
                query = search.Descending
                    ? query.OrderByDescending(v => v.PublishedAt).ThenByDescending(v => v.CreatedAt)
                    : query.OrderBy(v => v.PublishedAt).ThenByDescending(v => v.CreatedAt);
                break;
        }

        query = query.Paginate(search.PaginationQuery);

        return query;

    }

    public IQueryable<Video> WhereUserIsAllowedToAccessVideoOrVideoIsPublic(IQueryable<Video> query, Guid? userId)
    {
        if (userId != null)
        {
            return query.Where(v => v.PrivacyStatus == EPrivacyStatus.Public ||
                                    Ctx.EntityAccessPermissions.Any(p =>
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

    public VideoRepository(AppDbContext ctx) : base(ctx)
    {
    }
}