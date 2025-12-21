namespace RescueTube.WebApi.ApiModels;

public record EnqueueDataFetchJobRequestV1(
    string JobId,
    Guid EntityId);