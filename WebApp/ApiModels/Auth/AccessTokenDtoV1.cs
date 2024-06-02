namespace WebApp.ApiModels.Auth;

public class AccessTokenDtoV1
{
    public required string Token { get; set; }
    public required DateTimeOffset ExpiresAt { get; set; }
}