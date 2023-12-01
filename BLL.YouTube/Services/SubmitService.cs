using BLL.Contracts;
using BLL.Contracts.Exceptions;
using BLL.DTO.Entities;
using BLL.YouTube.Base;
using BLL.YouTube.Utils;
using DAL.EF.Extensions;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace BLL.YouTube.Services;

public class SubmitService : BaseYouTubeService, IPlatformSubmissionHandler
{
    public bool IsPlatformUrl(string url) => Url.IsYouTubeUrl(url);

    public async Task<LinkSubmissionSuccessResult> SubmitLink(string url, Guid submitterId, bool autoSubmit)
    {
        var isVideoUrl = Url.IsVideoUrl(url, out var videoId);
        if (isVideoUrl)
        {
            return await SubmitVideo(videoId!, submitterId, autoSubmit);
        }

        // TODO
        throw new UnrecognizedUrlException(url);
    }

    private async Task<LinkSubmissionSuccessResult> SubmitVideo(string idOnPlatform, Guid submitterId, bool autoSubmit)
    {
        var previouslyArchivedVideo = await Ctx.Videos.GetByIdOnPlatformAsync(idOnPlatform, EPlatform.YouTube);
        if (previouslyArchivedVideo != null)
        {
            return new LinkSubmissionSuccessResult(
                await ServiceUow.SubmissionService.Add(
                    previouslyArchivedVideo, submitterId, autoSubmit),
                true);
        }

        var videoData = await YouTubeUow.VideoService.FetchVideoDataYtdl(idOnPlatform, false);
        if (videoData == null) throw new VideoNotFoundOnPlatformException();

        if (!autoSubmit)
        {
            // TODO: Check for leftovers from this, in case of a race condition where video's existence was checked while it was in the process of being added
            return new LinkSubmissionSuccessResult(
                ServiceUow.SubmissionService.Add(
                    idOnPlatform, EPlatform.YouTube, EEntityType.Video, submitterId, autoSubmit),
                false);
        }

        var video = await YouTubeUow.VideoService.AddVideo(videoData);
        return new LinkSubmissionSuccessResult(await ServiceUow.SubmissionService.Add(
            video, submitterId, autoSubmit), false);
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