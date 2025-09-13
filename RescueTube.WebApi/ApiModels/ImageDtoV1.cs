using RescueTube.Domain.Enums;

namespace WebApp.ApiModels;

public class ImageDtoV1
{
    public Guid Id { get; set; }

    public required EPlatform Platform { get; set; }
    public string? IdOnPlatform { get; set; }

    public string? Key { get; set; }
    public string? Quality { get; set; }
    public string? Ext { get; set; }

    public string? OriginalUrl { get; set; }
    public string? LocalUrl { get; set; }
    public string? LocalFilePath { get; set; }
    public string? Url { get; set; }

    public int? Width { get; set; }
    public int? Height { get; set; }
}