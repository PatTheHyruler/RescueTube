using Domain.Entities;
using Domain.Enums;

namespace BLL.Data.Extensions;

public static class AuthorExtensions
{
    public static IQueryable<Author> Filter(this IQueryable<Author> query, EPlatform platform, IEnumerable<string> idsOnPlatform)
    {
        return query.Where(e => e.Platform == platform && idsOnPlatform.Contains(e.IdOnPlatform));
    }
}