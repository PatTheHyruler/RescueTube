using RescueTube.Core.DTO.Entities;
using RescueTube.Core.Utils.Pagination.Contracts;

namespace WebApp.ViewModels.Comment;

public class VideoCommentsViewModel : IPaginationQuery
{
    public required VideoComments VideoComments { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; } = 50;
    public int? Total { get; set; }
}