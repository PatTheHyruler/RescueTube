namespace WebApp.ApiModels;

public class CommentRootsResponseDtoV1
{
    public required PaginationResultDtoV1 PaginationResult { get; set; }
    public required IEnumerable<CommentDtoV1> Comments { get; set; }
}