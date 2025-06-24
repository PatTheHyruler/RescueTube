using LinqKit;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data.Specifications;
using RescueTube.Core.DTO.Enums;
using RescueTube.Domain.Entities;

namespace RescueTube.DAL.EF.Postgres.Specifications;

public class VideoSpecification : BaseDbService, IVideoSpecification
{
    public VideoSpecification(IServiceProvider services) : base(services)
    {
    }

    public IQueryable<Video> SearchVideos(IVideoSpecification.VideoSearchParams search)
    {
        IQueryable<Video> query = Ctx.Videos;
        var filter = search.Filter;

        if (filter?.Platform is not null)
        {
            query = query.Where(e => e.Platform == filter.Platform);
        }

        if (!string.IsNullOrEmpty(filter?.Name))
        {
            var nameQuery = '%' + Utils.EscapeWildcards(filter.Name) + '%';
            query = query.Where(e => e.Title!.Translations!
                .Any(t => Microsoft.EntityFrameworkCore.EF.Functions
                    .ILike(t.Content, nameQuery, "\\")));
            // TODO: SQLI?
        }

        if (!string.IsNullOrEmpty(filter?.Author))
        {
            var authorQuery = "%" + Utils.EscapeWildcards(filter.Author) + "%";
            query = query.Where(e => e.VideoAuthors!
                .Select(a => a.Author!.UserName + a.Author!.DisplayName)
                .Any(n => Microsoft.EntityFrameworkCore.EF.Functions.ILike(n, authorQuery)));
        }

        if (filter?.AuthorIds is { Length: > 0 })
        {
            query = query.Where(v => v.VideoAuthors!.Any(va => filter.AuthorIds.Contains(va.AuthorId)));
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
}