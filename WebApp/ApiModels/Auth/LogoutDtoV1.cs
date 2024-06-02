namespace WebApp.ApiModels.Auth;

public class LogoutDtoV1
{
    public required string Jwt { get; set; }
    public required string RefreshToken { get; set; }
}