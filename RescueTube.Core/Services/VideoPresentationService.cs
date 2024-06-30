using System.Security.Claims;
using RescueTube.Core.Identity.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Base;
using RescueTube.Core.Contracts;
using RescueTube.Core.Data.Mappers;
using RescueTube.Core.Data.Pagination;
using RescueTube.Core.Data.Specifications;
using RescueTube.Core.DTO.Entities;
using RescueTube.Core.DTO.Enums;
using RescueTube.Core.Utils.Pagination;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Services;

public class VideoPresentationService : BaseService
{
    private readonly EntityMapper _mapper;
    private readonly IEnumerable<IPlatformPresentationHandler> _presentationHandlers;

    public async Task<PaginationResponse<List<VideoSimple>>> SearchVideosAsync(
        EPlatform? platformQuery, string? nameQuery,
        string? authorQuery, ICollection<Guid>? categoryIds,
        ClaimsPrincipal user, Guid? userAuthorId,
        IPaginationQuery paginationQuery,
        EVideoSortingOptions sortingOptions, bool descending)
    {
        var userId = user.GetUserIdIfExists();
        var accessAllowed = AuthorizationService.IsAllowedToAccessAnyContentByRole(user);
        paginationQuery = paginationQuery.ToClamped();
        var videos = await DataUow.Videos.SearchVideos(new IVideoSpecification.VideoSearchParams
            {
                Platform = platformQuery,
                Name = nameQuery, Author = authorQuery,
                CategoryIds = categoryIds, UserId = userId,
                UserAuthorId = userAuthorId,
                AccessAllowed = accessAllowed,
                SortingOptions = sortingOptions, Descending = descending,
            })
            .Paginate(paginationQuery)
            .Select(_mapper.ToVideoSimple)
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
            .Select(_mapper.ToVideoSimple)
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

        foreach (var presentationHandler in _presentationHandlers)
        {
            if (!presentationHandler.CanHandle(video)) continue;
            presentationHandler.Handle(video);
            break;
        }
    }

    public VideoPresentationService(IServiceProvider services, ILogger<VideoPresentationService> logger,
        IEnumerable<IPlatformPresentationHandler> presentationHandlers, EntityMapper mapper) : base(services,
        logger)
    {
        _presentationHandlers = presentationHandlers;
        _mapper = mapper;
    }
}