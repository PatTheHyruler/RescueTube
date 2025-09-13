namespace RescueTube.WebApi.ApiModels;

public sealed class DataFetchesResponseDtoV1 : PaginationResultDtoV1
{
    public required DataFetchDtoV1[] DataFetches { get; init; }
}