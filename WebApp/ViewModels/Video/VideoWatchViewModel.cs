using BLL.DTO.Entities;

namespace WebApp.ViewModels.Video;

public class VideoWatchViewModel
{
    public required VideoSimple Video { get; set; }
    public bool EmbedView { get; set; }
}