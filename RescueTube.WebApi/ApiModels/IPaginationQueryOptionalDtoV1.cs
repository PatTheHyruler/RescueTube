using RescueTube.Core.Utils.Pagination;

namespace WebApp.ApiModels;

public interface IPaginationQueryOptionalDtoV1 : IPaginationQuery
{
    int IPaginationBase.Page => Page ?? DefaultPage;
    int IPaginationBase.Limit => Limit ?? DefaultLimit;

    new int? Page { get; }
    new int? Limit { get; }

    int DefaultPage { get; }
    int DefaultLimit { get; }
}