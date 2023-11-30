namespace Domain.Contracts;

public interface IHistoryEntity<TEntity> : IIdDatabaseEntity where TEntity : IIdDatabaseEntity
{
    public Guid CurrentId { get; set; }
    public TEntity? Current { get; set; }

    public DateTime? LastOfficialValidAt { get; set; }
    public DateTime LastValidAt { get; set; }
    public DateTime FirstNotValidAt { get; set; }
}