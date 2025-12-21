using RescueTube.Core.JobOrchestration;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.DTO;

public record JobDefinitionWithSettings
{
    public required JobDefinition JobDefinition { get; init; }
    public required JobSettings JobSettings { get; init; }

    public void Deconstruct(out JobDefinition jobDefinition, out JobSettings jobSettings)
    {
        jobDefinition = JobDefinition;
        jobSettings = JobSettings;
    }
}