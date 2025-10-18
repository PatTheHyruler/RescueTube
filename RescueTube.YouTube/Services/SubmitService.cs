using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Contracts;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.Exceptions;
using RescueTube.Domain;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Base;
using RescueTube.YouTube.Utils;

namespace RescueTube.YouTube.Services;

public class SubmitService : BaseYouTubeService, IPlatformSubmissionHandler
{
    private readonly AppDbContext _dbCtx;
    private readonly YouTubeServices _youTubeServices;
    private readonly DataFetchService _dataFetchService;

    public SubmitService(AppDbContext dbCtx, YouTubeServices youTubeServices, DataFetchService dataFetchService)
    {
        _dbCtx = dbCtx;
        _youTubeServices = youTubeServices;
        _dataFetchService = dataFetchService;
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

    public async Task HandleSubmissionAsync(Submission submission, CancellationToken ct)
    {
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
        var existingAuthor = await _dbCtx.Authors
            .Where(a => a.Platform == EPlatform.YouTube)
            .Where(existingAuthorFilter)
            .Include(a => a.ArchivalSettings)
            .FirstOrDefaultAsync(ct);

        var addedOrExistingAuthor = existingAuthor;
        if (addedOrExistingAuthor == null)
        {
            await using var dataFetchScope = await _dataFetchService.StartDataFetchAsync(
                YouTubeConstants.DataFetches.YouTubeExplode.Channel, entityId: null, ct);
            dataFetchScope.ThrowIfAlreadyFetching();

            var dataFetch = dataFetchScope.DataFetch;

            var channel = await _youTubeServices.AuthorService.FetchYouTubeExplodeChannelAsync(idOnPlatform, idType, ct);

            if (channel is null)
            {
                await _dataFetchService.UpdateDataFetchStatusAsync(dataFetch, DataFetchStatus.Failed,
                    message: "Author not found on platform");
                throw new ApplicationException("Author not found on platform");
            }

            _dataFetchService.CompleteDataFetch(dataFetch);

            addedOrExistingAuthor = await _youTubeServices.AuthorService.AddOrGetAuthor(channel, dataFetch, ct);

            dataFetch.AuthorId ??= addedOrExistingAuthor.Id;
            dataFetch.Author ??= addedOrExistingAuthor;
        }

        if (addedOrExistingAuthor.ArchivalSettings == null)
        {
            await _dbCtx.Entry(addedOrExistingAuthor).Reference(a => a.ArchivalSettings).LoadAsync(ct);
        }

        if (addedOrExistingAuthor.ArchivalSettings != null)
        {
            _dbCtx.Remove(addedOrExistingAuthor.ArchivalSettings);
        }

        addedOrExistingAuthor.ArchivalSettings =
            options ?? AuthorArchivalSettings.CreateDefaultArchivedAuthorSettings(); // TODO: Better logic for this
        _dbCtx.Add(addedOrExistingAuthor.ArchivalSettings);

        return addedOrExistingAuthor;
    }

    private async Task<Video> SubmitVideoAsync(string videoIdOnPlatform, CancellationToken ct)
    {
        var existingVideo = await _dbCtx.Videos
            .Where(v => v.Platform == EPlatform.YouTube && v.IdOnPlatform == videoIdOnPlatform)
            .FirstOrDefaultAsync(cancellationToken: ct);
        if (existingVideo != null)
        {
            return existingVideo;
        }

        var addedVideo = await _youTubeServices.VideoService.AddOrUpdateVideoAsync(videoIdOnPlatform, ct);
        return addedVideo ?? throw new VideoNotFoundOnPlatformException();
    }

    private async Task<Playlist> SubmitPlaylistAsync(string playlistIdOnPlatform, CancellationToken ct)
    {
        var existingPlaylist = await _dbCtx.Playlists
            .Where(p => p.Platform == EPlatform.YouTube && p.IdOnPlatform == playlistIdOnPlatform)
            .FirstOrDefaultAsync(ct);
        if (existingPlaylist != null)
        {
            return existingPlaylist;
        }

        var addedPlaylist = await _youTubeServices.PlaylistService.AddOrUpdatePlaylistAsync(playlistIdOnPlatform, ct);
        return addedPlaylist ?? throw new ApplicationException("Playlist not found on platform");
    }
}