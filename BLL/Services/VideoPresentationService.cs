using System.Security.Claims;
using BLL.Base;
using BLL.Contracts;
using BLL.DTO.Entities;
using BLL.DTO.Enums;
using BLL.DTO.Mappers;
using BLL.Identity.Services;
using BLL.Utils;
using DAL.EF.Extensions;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class VideoPresentationService : BaseService
{
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
        var skipAmount = PaginationUtils.PageToSkipAmount(limit, page);
        var videos = (await Ctx.Videos.SearchVideos(
            dbContext: Ctx,
            platform: platformQuery,
            name: nameQuery, author: authorQuery,
            categoryIds: categoryIds, userId: userId,
            userAuthorId: userAuthorId,
            accessAllowed: accessAllowed,
            skipAmount: skipAmount, limit: limit,
            sortingOptions: sortingOptions, descending: descending
        ).ProjectToVideoSimple().ToListAsync());
        foreach (var video in videos)
        {
            foreach (var presentationHandler in _videoPresentationHandlers)
            {
                if (!presentationHandler.CanHandle(video)) continue;
                presentationHandler.Handle(video);
                break;
            }
        }

        return videos;
    }

    public VideoPresentationService(IServiceProvider services, ILogger<VideoPresentationService> logger, IEnumerable<IPlatformVideoPresentationHandler> videoPresentationHandlers) : base(services, logger)
    {
        _videoPresentationHandlers = videoPresentationHandlers;
    }
}