using RescueTube.Domain;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Services.Interfaces;

public interface IPlatformVideoDownloadService
{
    bool IsLikelyThrottled();
    Task<string> DownloadVideoAsync(Video video, CancellationToken ct);

    DataFetchDefinition DataFetchDefinition { get; }
}