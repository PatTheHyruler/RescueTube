namespace WebApp.ApiModels.Auth;

public class UserDtoV1
{
    public required Guid Id { get; set; }
    public required string UserName { get; set; }
    public required string NormalizedUserName { get; set; }
    public required bool IsApproved { get; set; }
    public required IEnumerable<RoleDtoV1> Roles { get; set; }
}