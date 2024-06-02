using RescueTube.Domain.Enums;

namespace WebApp.ApiModels;

public class AuthorSimpleDtoV1
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? DisplayName { get; set; }
    public EPlatform Platform { get; set; }
    public required string IdOnPlatform { get; set; }
    public required IEnumerable<ImageDtoV1> ProfileImages { get; set; }
    public string? UrlOnPlatform { get; set; }
}