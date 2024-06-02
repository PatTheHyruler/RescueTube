using System.Security.Claims;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using RescueTube.Core.Identity.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Base;
using RescueTube.Core.Contracts;
using RescueTube.Core.Data.Repositories;
using RescueTube.Core.DTO.Entities;
using RescueTube.Core.DTO.Enums;
using RescueTube.Core.Utils.Pagination;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Services;

public class VideoPresentationService : BaseService
{
    private readonly IMapper _mapper;
    private readonly IEnumerable<IPlatformVideoPresentationHandler> _videoPresentationHandlers;

    public async Task<PaginationResponse<List<VideoSimple>>> SearchVideosAsync(
        EPlatform? platformQuery, string? nameQuery,
        string? authorQuery, ICollection<Guid>? categoryIds,
        ClaimsPrincipal user, Guid? userAuthorId,
        IPaginationQuery paginationQuery,
        EVideoSortingOptions sortingOptions, bool descending)
    {
        var userId = user.GetUserIdIfExists();
        var accessAllowed = AuthorizationService.IsAllowedToAccessVideoByRole(user);
        paginationQuery = paginationQuery.ToClamped();
        var videos = await DataUow.VideoRepo.SearchVideos(new IVideoRepository.VideoSearchParams
            {
                Platform = platformQuery,
                Name = nameQuery, Author = authorQuery,
                CategoryIds = categoryIds, UserId = userId,
                UserAuthorId = userAuthorId,
                AccessAllowed = accessAllowed,
                PaginationQuery = paginationQuery,
                SortingOptions = sortingOptions, Descending = descending,
            })
            // .Select(VideoMapper.VideoToVideoSimpleExpression)
            // .ProjectToVideoSimple()
            .ProjectTo<VideoSimple>(_mapper.ConfigurationProvider)
            .ToListAsync();
        MakePresentable(videos);

        return new PaginationResponse<List<VideoSimple>>
        {
            Result = videos,
            PaginationResult = paginationQuery.ToPaginationResult(videos.Count),
        };
    }

    public async Task<VideoSimple?> GetVideoSimple(Guid videoId)
    {
        var video = await DbCtx.Videos
            .Where(v => v.Id == videoId)
            .ProjectTo<VideoSimple>(_mapper.ConfigurationProvider)
            // .ProjectToVideoSimple()
            .FirstOrDefaultAsync();
        MakePresentable(video);
        return video;
    }

    public async Task<VideoFile?> GetVideoFileAsync(Guid videoId)
    {
        return await DbCtx.VideoFiles
            .Where(e => e.VideoId == videoId)
            .OrderByDescending(e => e.ValidSince)
            .FirstOrDefaultAsync();
    }

    private void MakePresentable(IEnumerable<VideoSimple> videos)
    {
        foreach (var video in videos)
        {
            MakePresentable(video);
        }
    }

    private void MakePresentable(VideoSimple? video)
    {
        if (video == null)
        {
            return;
        }

        foreach (var presentationHandler in _videoPresentationHandlers)
        {
            if (!presentationHandler.CanHandle(video)) continue;
            presentationHandler.Handle(video);
            break;
        }
    }

    public VideoPresentationService(IServiceProvider services, ILogger<VideoPresentationService> logger,
        IEnumerable<IPlatformVideoPresentationHandler> videoPresentationHandlers, IMapper mapper) : base(services,
        logger)
    {
        _videoPresentationHandlers = videoPresentationHandlers;
        _mapper = mapper;
    }
}