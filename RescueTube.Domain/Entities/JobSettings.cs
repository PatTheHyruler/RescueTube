using RescueTube.Domain.Contracts;

namespace RescueTube.Domain.Entities;

public sealed class PersistedJobSettings : JobSettings, IIdDatabaseEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public override bool IsDefault => false;
}

public class JobSettings
{
    public required string JobId { get; init; }

    public required bool IsEnabled { get; set; }
    public required string Cron { get; set; }

    public DataFetchJobSettings? DataFetchJobSettings { get; init; }

    public virtual bool IsDefault => true;

    public PersistedJobSettings CloneToPersistedJobSettings()
    {
        return new()
        {
            JobId = JobId,
            IsEnabled = IsEnabled,
            Cron = Cron,
            DataFetchJobSettings = DataFetchJobSettings is not null ? new DataFetchJobSettings
            {
                SuccessCutoffOffset = DataFetchJobSettings.SuccessCutoffOffset,
                FailureCutoffOffset = DataFetchJobSettings.FailureCutoffOffset,
            } : null,
        };
    }
}

public sealed class DataFetchJobSettings
{
    public required TimeSpan SuccessCutoffOffset { get; set; }
    public required TimeSpan FailureCutoffOffset { get; set; }

    public static readonly DataFetchJobSettings Default = new()
    {
        SuccessCutoffOffset = TimeSpan.MaxValue,
        FailureCutoffOffset = TimeSpan.FromDays(10),
    };
}