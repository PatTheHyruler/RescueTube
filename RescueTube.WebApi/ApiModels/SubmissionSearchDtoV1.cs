namespace RescueTube.WebApi.ApiModels;

public class SubmissionSearchDtoV1 : IPaginationQueryOptionalDtoV1
{
    public int? Page { get; init; }
    public int? Limit { get; init;  }

    public bool? Completed { get; init; }

    public CommaSeparatedOrderByPropertyArray? OrderBy { get; init; }

    public int DefaultPage => 0;
    public int DefaultLimit => 50;
}