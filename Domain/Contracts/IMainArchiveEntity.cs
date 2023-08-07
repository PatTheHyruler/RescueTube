using Contracts.Domain;

namespace Domain.Contracts;

public interface IMainArchiveEntity :
    IIdDatabaseEntity, IPlatformEntity, IPrivacyEntity,
    IFetchable, IArchiveDateEntity
{
}