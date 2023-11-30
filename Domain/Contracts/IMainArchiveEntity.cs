namespace Domain.Contracts;

public interface IMainArchiveEntity :
    IIdDatabaseEntity, IPlatformEntity, IPrivacyEntity,
    IFetchable, IArchiveDateEntity
{
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}