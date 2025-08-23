using RescueTube.Domain.Enums;

namespace RescueTube.Core.DTO.Entities;

public interface IPlaylistDto
{
    EPlatform Platform { get; }
    string IdOnPlatform { get; }

    AuthorSimple? Creator { get; }
    string? UrlOnPlatform { get; set; }
}