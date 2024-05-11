using Domain.Base;

namespace Domain.Entities.Localization;

public class TextTranslationKey : BaseIdDbEntity
{
    public ICollection<TextTranslation>? Translations { get; set; }
}