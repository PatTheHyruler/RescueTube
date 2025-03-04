using System.Diagnostics.CodeAnalysis;
using RescueTube.Domain;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Contracts;

public interface IPlatformSubmissionHandler : IPlatformService
{
    public bool IsPlatformUrl(string url, [NotNullWhen(true)] out RecognizedPlatformUrl? recognizedPlatformUrl);
    public Task HandleSubmissionAsync(Submission submission, CancellationToken ct);
}