using Base.Domain;
using Domain.Entities.Localization;
using Domain.Enums;

namespace Domain.Entities;

public class Category : AbstractIdDatabaseEntity
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