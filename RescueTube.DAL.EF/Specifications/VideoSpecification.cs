using LinqKit;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data.Specifications;
using RescueTube.Core.DTO.Enums;
using RescueTube.Domain.Entities;

namespace RescueTube.DAL.EF.Specifications;

public class VideoSpecification : BaseDbService, IVideoSpecification
{
    public IQueryable<Video> SearchVideos(IVideoSpecification.VideoSearchParams search)
    {
        IQueryable<Video> query = Ctx.Videos;

        if (search.Platform != null)
        {
            query = query.Where(e => e.Platform == search.Platform);
        }

        if (!string.IsNullOrEmpty(search.Name))
        {
            var nameQuery = '%' + Utils.EscapeWildcards(search.Name) + '%';
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
            query = query.AsExpandable().Where(
                DataUow.Permissions.IsUserAllowedToAccessVideoOrVideoIsPublic(search.UserId, true));
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
                    : query.OrderBy(v => v.PublishedAt).ThenBy(v => v.CreatedAt);
                break;
        }

        return query;
    }

    public VideoSpecification(IServiceProvider services) : base(services)
    {
    }
}