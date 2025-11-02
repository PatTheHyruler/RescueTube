using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities;

public sealed class JobSettings : BaseIdDbEntity
{
    public required string JobId { get; init; }

    public required bool IsEnabled { get; set; }
    public required string Cron { get; set; }
}