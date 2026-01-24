namespace RescueTube.WebApi.ApiModels;

public class SubmissionSearchResponseDtoV1
{
    public required PaginationResultDtoV1 PaginationResult { get; init; }
    public required IEnumerable<SubmissionDtoV1> Results { get; init; }
}