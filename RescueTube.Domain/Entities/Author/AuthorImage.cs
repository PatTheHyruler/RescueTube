using Domain.Base;
using Domain.Enums;

namespace Domain.Entities;

public class AuthorImage : BaseIdDbEntity
{
    public EImageType ImageType { get; set; }

    public DateTime? ValidSince { get; set; }
    public DateTime? ValidUntil { get; set; }
    public DateTime? LastFetched { get; set; }

    public Guid AuthorId { get; set; }
    public Author? Author { get; set; }
    public Guid ImageId { get; set; }
    public Image? Image { get; set; }
}