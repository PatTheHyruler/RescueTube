namespace RescueTube.WebApi.ApiModels;

public record EnqueueDataFetchJobRequestV1(
    string JobName,
    Guid EntityId);