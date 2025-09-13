using RescueTube.Domain.Enums;

namespace WebApp.ApiModels;

public record DataFetchJobDefinitionsResponseDtoV1(IEnumerable<DataFetchJobDefinitionDtoV1> JobDefinitions);

public record DataFetchJobDefinitionDtoV1(EEntityType EntityType, string JobName);