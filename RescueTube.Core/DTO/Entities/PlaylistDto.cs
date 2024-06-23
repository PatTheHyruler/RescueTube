using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Localization;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.DTO.Entities;

public class PlaylistDto<TVideo>
{
    public required ICollection<TextTranslation> Title { get; set; }
    public required ICollection<TextTranslation> Description { get; set; }

    public required List<Image> Thumbnails { get; set; }
    public Image? Thumbnail { get; set; }

    public required EPlatform Platform { get; set; }
    public required string IdOnPlatform { get; set; }

    public required AuthorSimple? Creator { get; set; }

    public EPrivacyStatus? PrivacyStatusOnPlatform { get; set; }
    public EPrivacyStatus PrivacyStatus { get; set; }
}