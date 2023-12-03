using BLL.DTO.Entities;
using Utils.Pagination.Contracts;

namespace WebApp.ViewModels.Comment;

public class VideoCommentsViewModel : IPaginationQuery
{
    public VideoComments VideoComments { get; set; } = default!;
    public int Page { get; set; }
    public int Limit { get; set; } = 50;
    public int? Total { get; set; }
}