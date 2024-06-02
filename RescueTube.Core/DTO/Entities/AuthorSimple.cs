using RescueTube.Domain.Base;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.DTO.Entities;

public class AuthorSimple : BaseIdDbEntity
{
    public string? UserName { get; set; }
    public string? DisplayName { get; set; }
    public EPlatform Platform { get; set; }
    public required string IdOnPlatform { get; set; }
    public required List<Image> ProfileImages { get; set; }
    public string? UrlOnPlatform { get; set; }
}