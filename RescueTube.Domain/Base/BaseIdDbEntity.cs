using RescueTube.Domain.Contracts;

namespace RescueTube.Domain.Base;

public abstract class BaseIdDbEntity : BaseIdDbEntity<Guid>, IIdDatabaseEntity
{
    protected BaseIdDbEntity()
    {
        Id = Ulid.NewUlid().ToGuid();
    }
}

public abstract class BaseIdDbEntity<TKey> : IIdDatabaseEntity<TKey>
    where TKey : struct, IEquatable<TKey>
{
    public TKey Id { get; set; }
}