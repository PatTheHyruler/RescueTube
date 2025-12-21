using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.Data.Extensions;
using RescueTube.Core.DataFetches;
using RescueTube.Core.Services;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Base;
using RescueTube.YouTube.Utils;
using YoutubeDLSharp.Metadata;

namespace RescueTube.YouTube.Services;

public class VideoService : BaseYouTubeService
{
    private readonly AppDbContext _dbCtx;
    private readonly EntityUpdateService _entityUpdateService;
    private readonly ILogger<VideoService> _logger;
    private readonly YouTubeServices _youTubeServices;
    private readonly DataFetchService _dataFetchService;

    public VideoService(ILogger<VideoService> logger, AppDbContext dbCtx, EntityUpdateService entityUpdateService, YouTubeServices youTubeServices, DataFetchService dataFetchService)
    {
        _logger = logger;
        _dbCtx = dbCtx;
        _entityUpdateService = entityUpdateService;
        _youTubeServices = youTubeServices;
        _dataFetchService = dataFetchService;
    }

    public async Task<VideoData?> FetchVideoDataYtdlAsync(string id, bool fetchComments, CancellationToken ct = default)
    {
        var videoResult = await _youTubeServices.YoutubeDl.RunVideoDataFetchAsync(
            Url.ToVideoUrl(id), fetchComments: fetchComments, ct: ct);
        if (videoResult is not { Success: true })
        {
            // TODO: Add status change if video exists in archive

            return null;
        }

        return videoResult.Data;
    }

    public async Task UpdateVideoAsync(Guid videoId, CancellationToken ct)
    {
        var idOnPlatform = await _dbCtx.Videos
            .Where(v => v.Id == videoId && v.Platform == EPlatform.YouTube)
            .Select(v => v.IdOnPlatform)
            .FirstAsync(ct);

        await using var dataFetchScope = await _dataFetchService.StartDataFetchAsync(
            YouTubeConstants.DataFetches.YtDlp.VideoPage, videoId, ct);
        dataFetchScope.ThrowIfAlreadyFetching();

        await AddOrUpdateVideoAsync(idOnPlatform, dataFetchScope, ct);
    }

    public async Task<Video?> AddOrUpdateVideoAsync(Submission submission, CancellationToken ct)
    {
        await using var dataFetchScope = await _dataFetchService.StartDataFetchAsync(
            YouTubeConstants.DataFetches.YtDlp.VideoPage, submission, ct);
        dataFetchScope.ThrowIfAlreadyFetching();

        return await AddOrUpdateVideoAsync(submission.IdOnPlatform, dataFetchScope, ct);
    }

    private async Task<Video?> AddOrUpdateVideoAsync(string idOnPlatform, DataFetchScope dataFetchScope, CancellationToken ct)
    {
        dataFetchScope.ThrowIfAlreadyFetching();

        var dataFetch = dataFetchScope.DataFetch;

        var videoResult = await _youTubeServices.YoutubeDl.RunVideoDataFetchAsync(
            Url.ToVideoUrl(idOnPlatform), fetchComments: false, ct: ct);
        if (videoResult is not { Success: true, Data: not null })
        {
            // TODO: Add status change if video exists in archive
            await _dataFetchService.UpdateDataFetchStatusAsync(dataFetch, DataFetchStatus.Failed,
                message: videoResult?.ErrorOutputToString());
            return null;
        }

        _dataFetchService.CompleteDataFetch(dataFetch);

        var video = await AddOrUpdateVideoAsync(videoResult.Data, dataFetch, ct);

        dataFetch.VideoId ??= video.Id;
        dataFetch.Video ??= video;

        return video;
    }

    public Task<Video> AddOrUpdateVideoAsync(VideoData videoData, DataFetch dataFetch, CancellationToken ct) =>
        AddOrUpdateVideoAsync(videoData, dataFetch, author: null, ct: ct);

    private async Task<Video> AddOrUpdateVideoAsync(VideoData videoData, DataFetch dataFetch, Author? author,
        CancellationToken ct = default)
    {
        var video = await _dbCtx.Videos
            .Where(v => v.Platform == EPlatform.YouTube && v.IdOnPlatform == videoData.ID)
            .Include(v => v.Title)
            .ThenInclude(t => t!.Translations)
            .Include(v => v.Description)
            .ThenInclude(t => t!.Translations)
            .Include(v => v.VideoTags)
            .Include(v => v.VideoStatisticSnapshots)
            .Include(v => v.Captions)
            .Include(v => v.VideoImages!)
            .ThenInclude(vi => vi.Image)
            .Include(v => v.VideoFiles)
            //Authors etc???
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken: ct);
        var isNew = video == null;
        video ??= new Video
        {
            IdOnPlatform = videoData.ID,
            ArchivalSettings = VideoArchivalSettings.CreateDefaultArchivedVideoSettings(),
        };

        _dbCtx.DataFetchResults.Add(new DataFetchResult { Video = video, DataFetch = dataFetch });

        var newVideoData = videoData.ToDomainVideo();
        _entityUpdateService.UpdateVideo(video, newVideoData, isNew, EntityUpdateService.EImageUpdateOptions.OnlyAdd);

        if (author == null)
        {
            await TryAddAuthorAsync(video, videoData, dataFetch, ct);
        }
        else
        {
            if (isNew)
            {
                _dbCtx.VideoAuthors.SetVideoAuthor(video.Id, author.Id);
            }
            else
            {
                await _youTubeServices.AuthorService.AddAndSetAuthor(video, author, ct);
            }
        }

        if (isNew)
        {
            _dbCtx.Videos.Add(video);
        }

        return video;
    }

    public async Task AddOrUpdateVideosFromAuthorVideosFetchAsync(VideoData authorData, Author author,
        DataFetch dataFetch, CancellationToken ct)
    {
        foreach (var fakePlaylistOrVideoData in authorData.Entries)
        {
            await HandleVideoDataFromAuthorVideosFetchAsync(fakePlaylistOrVideoData, author, dataFetch, ct);
        }
    }

    private Task HandleVideoDataFromAuthorVideosFetchAsync(
        VideoData fakePlaylistOrVideoData, Author author,
        DataFetch dataFetch,
        CancellationToken ct = default)
    {
        return HandleVideoDataFromAuthorVideosFetchAsync(
            fakePlaylistOrVideoData, author, dataFetch, parentVideoType: null, depth: 0, ct: ct);
    }

    private async Task HandleVideoDataFromAuthorVideosFetchAsync(VideoData fakePlaylistOrVideoData, Author author,
        DataFetch dataFetch,
        EVideoType? parentVideoType, uint depth,
        CancellationToken ct)
    {
        if (depth > 1)
        {
            _logger.LogCritical("Unexpectedly large recursion depth, skipping. Title: {VideoDataTitle}",
                fakePlaylistOrVideoData.Title);
            return;
        }

        if (fakePlaylistOrVideoData.Entries is not null)
        {
            EVideoType? videoType = null;
            if (fakePlaylistOrVideoData.Title?.EndsWith(" - Shorts") ?? false)
            {
                videoType = EVideoType.Short;
            }

            foreach (var data in fakePlaylistOrVideoData.Entries)
            {
                await HandleVideoDataFromAuthorVideosFetchAsync(data, author, dataFetch, videoType, depth + 1, ct);
            }
        }
        else
        {
            var video = await AddOrUpdateVideoAsync(fakePlaylistOrVideoData, dataFetch, author, ct);
            if (parentVideoType != null)
            {
                video.Type ??= parentVideoType;
            }
        }
    }

    private async Task TryAddAuthorAsync(Video video, VideoData videoData, DataFetch dataFetch,
        CancellationToken ct = default)
    {
        try
        {
            await _youTubeServices.AuthorService.AddAndSetAuthor(video, videoData, dataFetch, ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to add author for YouTube video {VideoId}, Author ID {AuthorId} ({AuthorName})",
                videoData.ID, videoData.ChannelID, videoData.Channel);
        }
    }
}