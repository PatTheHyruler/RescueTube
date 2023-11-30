using Domain.Base;
using Domain.Entities;
using Domain.Enums;

namespace BLL.DTO.Entities;

public class AuthorSimple : BaseIdDbEntity
{
    public string? UserName { get; set; }
    public string? DisplayName { get; set; }
    public EPlatform Platform { get; set; }
    public string IdOnPlatform { get; set; } = default!;
    public List<Image>? ProfileImages { get; set; }
    public string? UrlOnPlatform { get; set; }
}