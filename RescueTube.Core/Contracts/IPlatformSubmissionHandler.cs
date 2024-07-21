using System.Diagnostics.CodeAnalysis;
using RescueTube.Domain;

namespace RescueTube.Core.Contracts;

public interface IPlatformSubmissionHandler
{
    public bool IsPlatformUrl(string url, [NotNullWhen(true)] out RecognizedPlatformUrl? recognizedPlatformUrl);
}