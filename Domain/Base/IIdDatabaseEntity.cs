namespace Domain.Base;

public interface IIdDatabaseEntity : IIdDatabaseEntity<Guid>
{
}

public interface IIdDatabaseEntity<TId> where TId : struct, IEquatable<TId>
{
    public TId Id { get; set; }
}