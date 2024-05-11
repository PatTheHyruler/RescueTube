namespace RescueTube.Domain.Contracts;

public interface IMainArchiveEntity :
    IIdDatabaseEntity, IPlatformEntity, IPrivacyEntity,
    IFetchable, IArchiveDateEntity
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}