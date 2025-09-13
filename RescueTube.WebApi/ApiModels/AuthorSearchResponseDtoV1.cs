namespace RescueTube.WebApi.ApiModels;

public sealed class AuthorSearchResponseDtoV1 : PaginationResultDtoV1
{
    public required AuthorSimpleDtoV1[] Authors { get; init; }
}