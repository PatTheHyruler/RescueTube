namespace WebApp.ApiModels;

public class VideoSearchResponseDtoV1
{
    public required PaginationResultDtoV1 PaginationResult { get; set; }
    public required IEnumerable<VideoSimpleDtoV1> Videos { get; set; }
}