using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Data.Extensions;

public static class AuthorExtensions
{
    public static IQueryable<Author> Filter(this IQueryable<Author> query, EPlatform platform, IEnumerable<string> idsOnPlatform)
    {
        return query.Where(e => e.Platform == platform && idsOnPlatform.Contains(e.IdOnPlatform));
    }
}