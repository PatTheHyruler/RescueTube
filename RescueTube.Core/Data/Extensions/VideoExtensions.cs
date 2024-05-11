using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace RescueTube.Core.Data.Extensions;

public static class VideoExtensions
{
    public static IQueryable<Video>
        FilterByIdOnPlatform(this IQueryable<Video> query, string id, EPlatform platform) =>
        query.Where(e => e.Platform == platform && e.IdOnPlatform == id);

    public static Task<Video?> GetByIdOnPlatformAsync(this IQueryable<Video> query, string id, EPlatform platform) =>
        query.FilterByIdOnPlatform(id, platform)
            .FirstOrDefaultAsync();
}