using BLL.DTO.Entities;

namespace WebApp.ViewModels.Video;

public class VideoWatchViewModel
{
    public VideoSimple Video { get; set; } = default!;
    public bool EmbedView { get; set; } = false;
}