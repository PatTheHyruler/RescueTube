using BLL.DTO.Entities;

namespace WebApp.ViewModels.Video;

public class VideoSearchViewModel : VideoSearchQueryModel
{
    public List<VideoSimple> Videos { get; set; } = default!;
}