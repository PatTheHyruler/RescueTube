using RescueTube.Core.DTO.Entities;
using RescueTube.Core.Utils.Pagination;

namespace WebApp.ViewModels.Video;

public class VideoSearchViewModel : VideoSearchQueryModel
{
    public required List<VideoSimple> Videos { get; set; }
    public required IPaginationResult PaginationResult { get; set; }
}