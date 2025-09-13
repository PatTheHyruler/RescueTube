using System.Linq.Expressions;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data.Specifications;
using RescueTube.Core.DTO.Enums;
using RescueTube.Core.DTO.Videos;
using RescueTube.Domain.Entities;

namespace RescueTube.DAL.EF.Postgres.Specifications;

public class VideoSpecification : BaseDbService, IVideoSpecification
{
    public VideoSpecification(IServiceProvider services) : base(services)
    {
    }

    public Expression<Func<Video, bool>> FilterVideos(VideoSearchFilter filter)
    {
        Expression<Func<Video, bool>> query = v => true;

        if (filter.Platform is not null)
        {
            query = query.And(v => v.Platform == filter.Platform);
        }

        if (!string.IsNullOrEmpty(filter.Name))
        {
            var nameQuery = '%' + Utils.EscapeWildcards(filter.Name) + '%';
            query = query.And(v => v.Title!.Translations!
                .Any(t => Microsoft.EntityFrameworkCore.EF.Functions
                    .ILike(t.Content, nameQuery, "\\")));
            // TODO: SQLI?
        }

        if (!string.IsNullOrEmpty(filter.Author))
        {
            var authorQuery = "%" + Utils.EscapeWildcards(filter.Author) + "%";
            query = query.And(e => e.VideoAuthors!
                .Select(a => a.Author!.UserName + a.Author!.DisplayName)
                .Any(n => Microsoft.EntityFrameworkCore.EF.Functions.ILike(n, authorQuery)));
        }

        if (filter.AuthorIds is { Length: > 0 })
        {
            query = query.And(v => v.VideoAuthors!.Any(va => filter.AuthorIds.Contains(va.AuthorId)));
        }

        return query;
    }

    public IQueryable<Video> SearchVideos(IVideoSpecification.VideoSearchParams search)
    {
        IQueryable<Video> query = Ctx.Videos;

        if (search.Filter is not null)
        {
            query = query.AsExpandable().Where(FilterVideos(search.Filter));
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