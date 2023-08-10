using Base.Domain;
using Domain.Enums;

namespace Domain.Entities;

public class VideoImage : AbstractIdDatabaseEntity
{
    public EImageType ImageType { get; set; }

    public DateTime? ValidSince { get; set; }
    public DateTime? ValidUntil { get; set; }
    public DateTime? LastFetched { get; set; }
    
    public Guid VideoId { get; set; }
    public Video? Video { get; set; }

    public Guid ImageId { get; set; }
    public Image? Image { get; set; }
}