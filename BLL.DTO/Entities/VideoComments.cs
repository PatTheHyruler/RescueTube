using Domain.Entities;

namespace BLL.DTO.Entities;

public class VideoComments
{
    public Guid Id { get; set; }
    public DateTime? LastCommentsFetch { get; set; }
    public ICollection<Comment> Comments { get; set; } = default!;
}