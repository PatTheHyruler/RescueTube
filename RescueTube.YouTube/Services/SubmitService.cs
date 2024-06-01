using Microsoft.Extensions.Logging;
using RescueTube.Core.Contracts;
using RescueTube.Core.Data.Extensions;
using RescueTube.Core.DTO.Entities;
using RescueTube.Core.Exceptions;
using RescueTube.Domain.Enums;
using RescueTube.YouTube.Base;
using RescueTube.YouTube.Utils;

namespace RescueTube.YouTube.Services;

public class SubmitService : BaseYouTubeService, IPlatformSubmissionHandler
{
    public bool IsPlatformUrl(string url) => Url.IsYouTubeUrl(url);

    public async Task<LinkSubmissionSuccessResult> SubmitLink(string url, Guid submitterId, bool autoSubmit,
        CancellationToken ct = default)
    {
        var isVideoUrl = Url.IsVideoUrl(url, out var videoId);
        if (isVideoUrl)
        {
            return await SubmitVideo(videoId!, submitterId, autoSubmit, ct);
        }

        // TODO
        throw new UnrecognizedUrlException(url);
    }

    private async Task<LinkSubmissionSuccessResult> SubmitVideo(string idOnPlatform, Guid submitterId, bool autoSubmit,
        CancellationToken ct = default)
    {
        var previouslyArchivedVideo = await DbCtx.Videos.GetByIdOnPlatformAsync(idOnPlatform, EPlatform.YouTube, ct);
        if (previouslyArchivedVideo != null)
        {
            return new LinkSubmissionSuccessResult(
                await ServiceUow.SubmissionService.Add(
                    previouslyArchivedVideo, submitterId, autoSubmit, ct),
                true);
        }

        var videoData = await YouTubeUow.VideoService.FetchVideoDataYtdl(idOnPlatform, false, ct);
        if (videoData == null) throw new VideoNotFoundOnPlatformException();

        if (!autoSubmit)
        {
            // TODO: Check for leftovers from this, in case of a race condition where video's existence was checked while it was in the process of being added
            return new LinkSubmissionSuccessResult(
                ServiceUow.SubmissionService.Add(
                    idOnPlatform, EPlatform.YouTube, EEntityType.Video, submitterId, autoSubmit),
                false);
        }

        var video = await YouTubeUow.VideoService.AddVideo(videoData, ct);
        return new LinkSubmissionSuccessResult(await ServiceUow.SubmissionService.Add(
            video, submitterId, autoSubmit, ct), false);
    }

    public bool CanHandle(EPlatform platform, EEntityType entityType)
    {
        return platform == EPlatform.YouTube &&
               entityType is EEntityType.Video; // TODO
    }

    public SubmitService(IServiceProvider services, ILogger<SubmitService> logger) : base(services, logger)
    {
    }
}