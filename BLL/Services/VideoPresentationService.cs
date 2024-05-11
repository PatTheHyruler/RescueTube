using System.Security.Claims;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BLL.Base;
using BLL.Contracts;
using BLL.Data.Repositories;
using BLL.DTO.Entities;
using BLL.DTO.Enums;
using BLL.Identity.Services;
using BLL.Utils.Pagination;
using BLL.Utils.Pagination.Contracts;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class VideoPresentationService : BaseService
{
    private readonly IMapper _mapper;
    private readonly IEnumerable<IPlatformVideoPresentationHandler> _videoPresentationHandlers;

    public async Task<List<VideoSimple>> SearchVideosAsync(
        EPlatform? platformQuery, string? nameQuery,
        string? authorQuery, ICollection<Guid>? categoryIds,
        ClaimsPrincipal user, Guid? userAuthorId,
        int page, int limit,
        EVideoSortingOptions sortingOptions, bool descending)
    {
        var userId = user.GetUserIdIfExists();
        var accessAllowed = AuthorizationService.IsAllowedToAccessVideoByRole(user);
        int? total = null;
        PaginationUtils.ConformValues(ref total, ref limit, ref page);
        var videos = await DataUow.VideoRepo.SearchVideos(new IVideoRepository.VideoSearchParams
            {
                Platform = platformQuery,
                Name = nameQuery, Author = authorQuery,
                CategoryIds = categoryIds, UserId = userId,
                UserAuthorId = userAuthorId,
                AccessAllowed = accessAllowed,
                PaginationQuery = new PaginationQuery { Page = page, Limit = limit, Total = total },
                SortingOptions = sortingOptions, Descending = descending,
            })
            // .Select(VideoMapper.VideoToVideoSimpleExpression)
            // .ProjectToVideoSimple()
            .ProjectTo<VideoSimple>(_mapper.ConfigurationProvider)
            .ToListAsync();
        MakePresentable(videos);

        return videos;
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