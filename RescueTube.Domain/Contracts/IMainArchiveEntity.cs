namespace RescueTube.Domain.Contracts;

public interface IMainArchiveEntity :
    IIdDatabaseEntity, IPlatformEntity, IPrivacyEntity, IArchiveDateEntity
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}