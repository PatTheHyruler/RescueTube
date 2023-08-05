namespace BLL.DTO.Entities.Identity;

public class JwtResult
{
    public string Jwt { get; set; } = default!;
    public RefreshToken RefreshToken { get; set; } = default!;
}