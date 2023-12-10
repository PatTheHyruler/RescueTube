using BLL.DTO.Entities;

namespace WebApp.ViewModels.Video;

public class VideoSearchViewModel : VideoSearchQueryModel
{
    public required List<VideoSimple> Videos { get; set; }
}