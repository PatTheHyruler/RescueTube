namespace RescueTube.Core.DTO.Entities.Identity;

public class JwtResult
{
    public required string Jwt { get; set; }
    public required RefreshToken RefreshToken { get; set; }
}