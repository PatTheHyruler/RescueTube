using Microsoft.EntityFrameworkCore;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Data.Extensions;

public static class VideoAuthorExtensions
{
    public static async Task SetVideoAuthorIfNotSet(this DbSet<VideoAuthor> dbSet, Guid videoId, Guid authorId, EAuthorRole role = EAuthorRole.Publisher)
    {
        if (await dbSet.Filter(videoId, authorId, role).AnyAsync())
        {
            return;
        }

        dbSet.SetVideoAuthor(videoId, authorId, role);
    }

    public static void SetVideoAuthor(this DbSet<VideoAuthor> dbSet, Guid videoId, Guid authorId,
        EAuthorRole role = EAuthorRole.Publisher)
    {
        dbSet.Add(new VideoAuthor
        {
            VideoId = videoId,
            AuthorId = authorId,
            Role = role,
        });
    }

    public static IQueryable<VideoAuthor> Filter(this IQueryable<VideoAuthor> query,
        Guid videoId, Guid authorId, EAuthorRole? role = null)
    {
        query = query.Where(va => va.VideoId == videoId && va.AuthorId == authorId);
        if (role != null)
        {
            query = query.Where(va => va.Role == role);
        }

        return query;
    }

}