using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Contracts;
using RescueTube.Core.DTO.Entities;
using RescueTube.Core.Exceptions;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Base;
using RescueTube.YouTube.Utils;

namespace RescueTube.YouTube.Services;

public class SubmitService : BaseYouTubeService, IPlatformSubmissionHandler
{
    public SubmitService(IServiceProvider services, ILogger<SubmitService> logger) : base(services, logger)
    {
    }

    public bool IsPlatformUrl(string url, [NotNullWhen(true)] out RecognizedPlatformUrl? recognizedPlatformUrl)
    {
        if (Url.IsVideoUrl(url, out var videoId))
        {
            recognizedPlatformUrl = new RecognizedPlatformUrl(url, videoId, EPlatform.YouTube, EEntityType.Video);
            return true;
        }

        if (Url.IsPlaylistUrl(url, out var playlistId))
        {
            recognizedPlatformUrl = new RecognizedPlatformUrl(url, playlistId, EPlatform.YouTube, EEntityType.Playlist);
            return true;
        }

        recognizedPlatformUrl = null;
        return false;
    }

    public async Task HandleSubmissionAsync(Guid submissionId, CancellationToken ct = default)
    {
        var submission = await DbCtx.Submissions
            .Where(s => s.Id == submissionId)
            .FirstOrDefaultAsync(cancellationToken: ct);

        // TODO: Use fluent validation maybe?
        submission = submission switch
        {
            null => throw new ApplicationException("Submission not found"),
            { ApprovedAt: null } => throw new ApplicationException("Submission not approved"),
            { Platform: not EPlatform.YouTube } => throw new ApplicationException(
                $"Invalid platform {submission.Platform}, expected {EPlatform.YouTube}"),
            _ => submission,
        };

        if (submission.CompletedAt != null)
        {
            Logger.LogInformation("Submission {SubmissionId} already handled at {CompletedAt}, skipping",
                submissionId, submission.CompletedAt);
            return;
        }

        switch (submission.EntityType)
        {
            case EEntityType.Video:
                var video = await SubmitVideoAsync(submission.IdOnPlatform, ct);
                submission.VideoId = video.Id;
                break;
            default:
                throw new ApplicationException($"Unsupported entity type {submission.EntityType}");
        }

        submission.CompletedAt = DateTimeOffset.UtcNow;
    }

    private async Task<Video> SubmitVideoAsync(string videoIdOnPlatform, CancellationToken ct)
    {
        var existingVideo = await DbCtx.Videos
            .Where(v => v.Platform == EPlatform.YouTube && v.IdOnPlatform == videoIdOnPlatform)
            .FirstOrDefaultAsync(cancellationToken: ct);
        if (existingVideo != null)
        {
            return existingVideo;
        }

        var addedVideo = await YouTubeUow.VideoService.AddVideoAsync(videoIdOnPlatform, ct);
        return addedVideo ?? throw new VideoNotFoundOnPlatformException();
    }
}