namespace WebApp.ApiModels.Auth;

public class RoleDtoV1
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
}