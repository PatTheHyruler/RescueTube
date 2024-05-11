namespace RescueTube.Domain.Contracts;

public interface IHistoryEntity<TEntity> : IIdDatabaseEntity where TEntity : IIdDatabaseEntity
{
    public Guid CurrentId { get; set; }
    public TEntity? Current { get; set; }

    public DateTimeOffset? LastOfficialValidAt { get; set; }
    public DateTimeOffset LastValidAt { get; set; }
    public DateTimeOffset FirstNotValidAt { get; set; }
}