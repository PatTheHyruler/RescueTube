using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Contracts;
using RescueTube.Core.Data;
using RescueTube.Core.Data.Mappers;
using RescueTube.Core.Data.Pagination;
using RescueTube.Core.Data.Specifications;
using RescueTube.Core.DTO.Entities;
using RescueTube.Core.DTO.Enums;
using RescueTube.Core.DTO.Videos;
using RescueTube.Core.Identity.Services;
using RescueTube.Core.Utils.Pagination;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Services;

public class VideoPresentationService
{
    private readonly AppDbContext _dbCtx;
    private readonly IDataUow _dataUow;
    private readonly EntityMapper _mapper;
    private readonly IEnumerable<IPlatformPresentationHandler> _presentationHandlers;

    public VideoPresentationService(AppDbContext dbCtx, IDataUow dataUow, EntityMapper mapper, IEnumerable<IPlatformPresentationHandler> presentationHandlers)
    {
        _dbCtx = dbCtx;
        _dataUow = dataUow;
        _mapper = mapper;
        _presentationHandlers = presentationHandlers;
    }

    public async Task<PaginationResponse<List<VideoSimple>>> SearchVideosAsync(
        VideoSearchFilter filter,
        ClaimsPrincipal user,
        IPaginationQuery paginationQuery,
        EVideoSortingOptions sortingOptions, bool descending,
        CancellationToken ct)
    {
        var userId = user.GetUserIdIfExists();
        paginationQuery = paginationQuery.ToClamped();
        var videos = await _dataUow.Videos.SearchVideos(new IVideoSpecification.VideoSearchParams
            {
                Filter = filter,
                UserId = userId,
                SortingOptions = sortingOptions, Descending = descending,
            })
            .Paginate(paginationQuery)
            .Select(_mapper.ToVideoSimple)
            .AsSplitQuery()
            .ToListAsync(ct);
        MakePresentable(videos);

        return new PaginationResponse<List<VideoSimple>>
        {
            Result = videos,
            PaginationResult = paginationQuery.ToPaginationResult(videos.Count),
        };
    }

    public async Task<VideoSimple?> GetVideoSimpleAsync(Guid videoId, CancellationToken ct)
    {
        var video = await _dbCtx.Videos
            .Where(v => v.Id == videoId)
            .Select(_mapper.ToVideoSimple)
            .FirstOrDefaultAsync(ct);
        MakePresentable(video);
        return video;
    }

    public async Task<VideoFile?> GetVideoFileAsync(Guid videoId, CancellationToken ct = default)
    {
        return await _dbCtx.VideoFiles
            .Where(e => e.VideoId == videoId)
            .OrderByDescending(e => e.ValidSince)
            .FirstOrDefaultAsync(ct);
    }

    private void MakePresentable(IEnumerable<VideoSimple> videos)
    {
        foreach (var video in videos)
        {
            MakePresentable(video);
        }
    }

    public void MakePresentable(VideoSimple? video)
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
}