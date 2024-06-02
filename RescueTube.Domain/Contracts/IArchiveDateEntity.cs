namespace RescueTube.Domain.Contracts;

public interface IArchiveDateEntity
{
    public DateTimeOffset AddedToArchiveAt { get; set; }
}