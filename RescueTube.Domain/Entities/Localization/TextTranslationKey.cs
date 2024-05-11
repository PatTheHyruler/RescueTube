using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities.Localization;

public class TextTranslationKey : BaseIdDbEntity
{
    public ICollection<TextTranslation>? Translations { get; set; }
}