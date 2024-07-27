using RescueTube.Domain.Entities;

namespace RescueTube.Domain.Contracts;

public interface IFetchable
{
    public ICollection<DataFetch>? DataFetches { get; set; }
}