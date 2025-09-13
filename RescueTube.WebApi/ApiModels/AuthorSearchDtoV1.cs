namespace RescueTube.WebApi.ApiModels;

public class AuthorSearchDtoV1 : IPaginationQueryOptionalDtoV1
{
    public int? Page { get; init; }
    public int? Limit { get; init; }

    public int DefaultPage => 0;
    public int DefaultLimit => 50;

    public string? Name { get; init; }
    public CommaSeparatedGuidArray? AuthorIds { get; init; }
    public CommaSeparatedGuidArray? ExcludeAuthorIds { get; init; }
}