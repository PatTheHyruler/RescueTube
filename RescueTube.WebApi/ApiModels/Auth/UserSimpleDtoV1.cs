namespace RescueTube.WebApi.ApiModels.Auth;

public record UserSimpleDtoV1
{
    public required Guid Id { get; init; }
    public required string UserName { get; init; }
}