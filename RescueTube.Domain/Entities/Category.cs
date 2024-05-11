using RescueTube.Domain.Base;
using RescueTube.Domain.Entities.Localization;
using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Entities;

public class Category : BaseIdDbEntity
{
    public Guid NameId { get; set; }
    public TextTranslationKey? Name { get; set; }

    public bool IsPublic { get; set; }
    public bool IsAssignable { get; set; }

    public EPlatform Platform { get; set; }
    public string? IdOnPlatform { get; set; }

    public Guid? CreatorId { get; set; }
    public Author? Creator { get; set; }

    public ICollection<VideoCategory>? VideoCategories { get; set; }
}