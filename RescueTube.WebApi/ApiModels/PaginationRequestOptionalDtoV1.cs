namespace RescueTube.WebApi.ApiModels;

public record PaginationRequestOptionalDtoV1 : IPaginationQueryOptionalDtoV1
{
    public int? Page { get; init; }
    public int? Limit { get; init;  }

    public int DefaultPage => 0;
    public int DefaultLimit => 50;
}