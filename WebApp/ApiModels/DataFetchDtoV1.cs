namespace WebApp.ApiModels;

public class DataFetchDtoV1
{
    public Guid Id { get; set; }
    public required DateTimeOffset OccurredAt { get; set; }
    public required bool Success { get; set; }
    public required string Type { get; set; }
    public required bool ShouldAffectValidity { get; set; }
    public required string Source { get; set; }
}