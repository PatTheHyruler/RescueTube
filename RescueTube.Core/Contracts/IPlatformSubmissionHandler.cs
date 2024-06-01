using RescueTube.Core.DTO.Entities;
using RescueTube.Core.Exceptions;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Contracts;

public interface IPlatformSubmissionHandler
{
    public bool IsPlatformUrl(string url);

    /// <summary>
    /// Submit an URL to the archive.
    /// </summary>
    /// <param name="url">The URL to submit.</param>
    /// <param name="submitterId">ID of the user submitting the URL.</param>
    /// <param name="autoSubmit">Whether the submission should be completed automatically without waiting for approval.</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="UnrecognizedUrlException">
    /// Provided <paramref name="url"/> was not recognized as a valid URL that is supported by the archive.
    /// </exception>
    /// <exception cref="VideoNotFoundOnPlatformException">
    /// Video identified by <paramref name="url"/> was not found on its source platform.
    /// </exception>
    public Task<LinkSubmissionSuccessResult> SubmitLink(string url, Guid submitterId, bool autoSubmit,
        CancellationToken ct = default);

    public bool CanHandle(EPlatform platform, EEntityType entityType);
}