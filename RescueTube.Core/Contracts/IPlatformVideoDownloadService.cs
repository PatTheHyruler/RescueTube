using RescueTube.Domain;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Contracts;

public interface IPlatformVideoDownloadService
{
    bool IsLikelyThrottled();
    Task<string> DownloadVideoAsync(Video video, CancellationToken ct);

    DataFetchDefinition DataFetchDefinition { get; }
}