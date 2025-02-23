using RescueTube.Domain.Contracts;

namespace RescueTube.Domain.Base;

public abstract class BaseIdDbEntity : BaseIdDbEntity<Guid>, IIdDatabaseEntity
{
    protected BaseIdDbEntity()
    {
        Id = Guid.CreateVersion7();
    }
}

public abstract class BaseIdDbEntity<TKey> : IIdDatabaseEntity<TKey>
    where TKey : struct, IEquatable<TKey>
{
    public TKey Id { get; set; }
}