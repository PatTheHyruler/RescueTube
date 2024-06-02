using RescueTube.Core.Utils.Pagination;

namespace WebApp.ViewModels.Comment;

public class VideoCommentsQueryViewModel : IPaginationQuery
{
    public int Page { get; set; }
    public int Limit { get; set; } = 50;
}