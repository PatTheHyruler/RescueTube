namespace WebApp.ApiModels;

public class LogoutDtoV1
{
    public required string Jwt { get; set; }
    public required string RefreshToken { get; set; }
}