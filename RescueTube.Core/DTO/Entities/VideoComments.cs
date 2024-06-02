namespace RescueTube.Core.DTO.Entities;

public class VideoComments
{
    public Guid Id { get; set; }
    public DateTimeOffset? LastCommentsFetch { get; set; }
    public required ICollection<CommentDto> Comments { get; set; }
}