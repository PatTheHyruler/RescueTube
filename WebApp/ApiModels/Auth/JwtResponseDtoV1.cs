namespace WebApp.ApiModels.Auth;

public class JwtResponseDtoV1
{
    public required string Jwt { get; set; }
    public required string RefreshToken { get; set; }
    public required DateTimeOffset RefreshTokenExpiresAt { get; set; }
}