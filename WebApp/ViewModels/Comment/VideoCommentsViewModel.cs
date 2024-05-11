using RescueTube.Core.DTO.Entities;
using RescueTube.Core.Utils.Pagination;

namespace WebApp.ViewModels.Comment;

public class VideoCommentsViewModel : VideoCommentsQueryViewModel
{
    public required VideoComments VideoComments { get; set; }
    public required IPaginationResult PaginationResult { get; set; }
}