using System.Security.Claims;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BLL.Base;
using BLL.Contracts;
using BLL.DTO.Entities;
using BLL.DTO.Enums;
using BLL.DTO.Mappers;
using BLL.Identity.Services;
using DAL.EF.Extensions;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utils.Pagination;
using Utils.Pagination.Contracts;

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
        var videos = await Ctx.Videos.SearchVideos(
            dbContext: Ctx,
            platform: platformQuery,
            name: nameQuery, author: authorQuery,
            categoryIds: categoryIds, userId: userId,
            userAuthorId: userAuthorId,
            accessAllowed: accessAllowed,
            new PaginationQuery { Page = page, Limit = limit, Total = total },
            sortingOptions: sortingOptions, descending: descending
        )
            // .Select(VideoMapper.VideoToVideoSimpleExpression)
            // .ProjectToVideoSimple()
            .ProjectTo<VideoSimple>(_mapper.ConfigurationProvider)
            .ToListAsync();
        MakePresentable(videos);

        Console.WriteLine("õüoi" + videos.FirstOrDefault()?.Authors.FirstOrDefault()?.ProfileImages?.FirstOrDefault()?.Id);
        
        return videos;
    }

    public async Task<VideoSimple?> GetVideoSimple(Guid videoId)
    {
        var video = await Ctx.Videos
            .Where(v => v.Id == videoId)
            .ProjectTo<VideoSimple>(_mapper.ConfigurationProvider)
            // .ProjectToVideoSimple()
            .FirstOrDefaultAsync();
        MakePresentable(video);
        return video;
    }

    public async Task<VideoFile?> GetVideoFileAsync(Guid videoId)
    {
        return await Ctx.VideoFiles
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
        IEnumerable<IPlatformVideoPresentationHandler> videoPresentationHandlers, IMapper mapper) : base(services, logger)
    {
        _videoPresentationHandlers = videoPresentationHandlers;
        _mapper = mapper;
    }
}