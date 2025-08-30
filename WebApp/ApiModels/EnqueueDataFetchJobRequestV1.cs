namespace WebApp.ApiModels;

public record EnqueueDataFetchJobRequestV1(
    string JobName,
    Guid EntityId);