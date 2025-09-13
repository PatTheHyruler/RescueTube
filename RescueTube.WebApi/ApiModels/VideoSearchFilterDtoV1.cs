namespace RescueTube.WebApi.ApiModels;

public class VideoSearchFilterDtoV1
{
    public string? NameQuery { get; init; }
    public string? AuthorQuery { get; init; }
    public Guid[]? AuthorIds { get; init; }
}
