using RescueTube.Domain.Entities;

namespace RescueTube.Domain.Contracts;

public interface IEntityImage : IIdDatabaseEntity
{
    public DateTimeOffset? ValidSince { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public DateTimeOffset? LastFetched { get; set; }

    public Guid ImageId { get; set; }
    public Image? Image { get; set; }
}