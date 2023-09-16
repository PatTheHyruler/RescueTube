using BLL.DTO.Entities;
using Domain.Entities;
using Domain.Enums;

namespace BLL.DTO.Mappers;

public static class VideoMapper
{
    public static IQueryable<VideoSimple> ProjectToVideoSimple(this IQueryable<Video> videos)
    {
        #nullable disable
        return videos.Select(v => new VideoSimple
        {
            Id = v.Id,
            Title = v.Title.Translations,
            Description = v.Description.Translations,
            Thumbnails = v.VideoImages
                .Where(vi => vi.ImageType == EImageType.Thumbnail)
                .Select(vi => vi.Image)
                .ToList(),
            Authors = v.VideoAuthors
                .Select(va => new AuthorSimple
            {
                Id = va.Author.Id,
                UserName = va.Author.UserName,
                DisplayName = va.Author.DisplayName,
                Platform = va.Author.Platform,
                IdOnPlatform = va.Author.IdOnPlatform,
                ProfileImages = va.Author.AuthorImages
                    .Where(ai => ai.ImageType == EImageType.ProfilePicture)
                    .Select(ai => ai.Image)
                    .ToList(),
            }).ToList(),

            Duration = v.Duration,
            Platform = v.Platform,
            IdOnPlatform = v.IdOnPlatform,
            CreatedAt = v.CreatedAt,
            PublishedAt = v.PublishedAt,
            AddedToArchiveAt = v.AddedToArchiveAt,
        });
    }
}