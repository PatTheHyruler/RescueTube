using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Contracts;
using RescueTube.Core.Events;
using RescueTube.Core.Exceptions;
using RescueTube.Domain;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Base;
using RescueTube.YouTube.Utils;

namespace RescueTube.YouTube.Services;

public class SubmitService : BaseYouTubeService, IPlatformSubmissionHandler
{
    private readonly IMediator _mediator;

    public SubmitService(IServiceProvider services, ILogger<SubmitService> logger, IMediator mediator) : base(services,
        logger)
    {
        _mediator = mediator;
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

        if (Url.IsAuthorHandleUrl(url, out var authorHandle))
        {
            recognizedPlatformUrl = new RecognizedPlatformUrl(url, authorHandle, EPlatform.YouTube, EEntityType.Author)
            {
                IdType = YouTubeConstants.IdTypes.Author.Handle,
            };
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
            case EEntityType.Playlist:
                var playlist = await SubmitPlaylistAsync(submission.IdOnPlatform, ct);
                submission.PlaylistId = playlist.Id;
                break;
            case EEntityType.Author:
                var author = await SubmitAuthorAsync(submission.IdOnPlatform, submission.IdType, options: null, ct: ct);
                submission.AuthorId = author.Id;
                break;
            default:
                throw new ApplicationException($"Unsupported entity type {submission.EntityType}");
        }

        submission.CompletedAt = DateTimeOffset.UtcNow;

        DataUow.RegisterSavedChangesCallbackRunOnce(() => _mediator.Publish(
            new SubmissionHandledEvent
            {
                SubmissionId = submissionId,
                Platform = submission.Platform,
                EntityType = submission.EntityType,
            },
            ct
        ));
    }

    private async Task<Author> SubmitAuthorAsync(string idOnPlatform, string? idType,
        AuthorArchivalSettings? options = null,
        CancellationToken ct = default)
    {
        Expression<Func<Author, bool>> existingAuthorFilter = idType switch
        {
            YouTubeConstants.IdTypes.Author.Handle => author => author.UserName == idOnPlatform,
            null => author => author.IdOnPlatform == idOnPlatform,
            _ => throw new ArgumentException($"Unsupported ID type '{idType}'", nameof(idType)),
        };
        var existingAuthor = await DbCtx.Authors
            .Where(a => a.Platform == EPlatform.YouTube)
            .Where(existingAuthorFilter)
            .FirstOrDefaultAsync(ct);

        if (existingAuthor != null)
        {
            return existingAuthor;
        }

        var channel = idType switch
        {
            YouTubeConstants.IdTypes.Author.Handle => await YouTubeUow.YouTubeExplodeClient.Channels.GetByHandleAsync(idOnPlatform,
                ct),
            _ => await YouTubeUow.YouTubeExplodeClient.Channels.GetAsync(idOnPlatform, ct),
        };

        var addedOrExistingAuthor = await YouTubeUow.AuthorService.AddOrGetAuthor(channel, ct);
        if (addedOrExistingAuthor == null)
        {
            throw new ApplicationException("Author not found on platform");
        }

        addedOrExistingAuthor.ArchivalSettings = options ?? AuthorArchivalSettings.ArchivedDefault(); // TODO: Better logic for this
        DataUow.RegisterSavedChangesCallbackRunOnce(() =>
            _mediator.Publish(new AuthorArchivalEnabledEvent
            {
                AuthorId = addedOrExistingAuthor.Id,
                Platform = EPlatform.YouTube,
                AuthorArchivalSettings = addedOrExistingAuthor.ArchivalSettings,
            }, ct));

        return addedOrExistingAuthor;
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

        var addedVideo = await YouTubeUow.VideoService.AddOrUpdateVideoAsync(videoIdOnPlatform, ct);
        return addedVideo ?? throw new VideoNotFoundOnPlatformException();
    }

    private async Task<Playlist> SubmitPlaylistAsync(string playlistIdOnPlatform, CancellationToken ct)
    {
        var existingPlaylist = await DbCtx.Playlists
            .Where(p => p.Platform == EPlatform.YouTube && p.IdOnPlatform == playlistIdOnPlatform)
            .FirstOrDefaultAsync(ct);
        if (existingPlaylist != null)
        {
            return existingPlaylist;
        }

        var addedPlaylist = await YouTubeUow.PlaylistService.AddPlaylistAsync(playlistIdOnPlatform, ct);
        return addedPlaylist ?? throw new ApplicationException("Playlist not found on platform");
    }
}