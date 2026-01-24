namespace RescueTube.Core.Utils.Pagination;

public interface IOrderByProperty
{
    string PropertyName { get; }
    bool Descending { get; }
}