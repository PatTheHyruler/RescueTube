using Domain.Contracts;

namespace Domain.Base;

public abstract class BaseIdDbEntity : BaseIdDbEntity<Guid>, IIdDatabaseEntity
{
    protected BaseIdDbEntity()
    {
        Id = Guid.NewGuid();
    }
}

public abstract class BaseIdDbEntity<TKey> : IIdDatabaseEntity<TKey>
    where TKey : struct, IEquatable<TKey>
{
    public TKey Id { get; set; }
}