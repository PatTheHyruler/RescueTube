using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Contracts;
using RescueTube.Core.Data;
using RescueTube.Core.Data.Mappers;
using RescueTube.Core.Data.Pagination;
using RescueTube.Core.Data.Specifications;
using RescueTube.Core.DTO.Entities;
using RescueTube.Core.Identity.Services;
using RescueTube.Core.Utils.Pagination;

namespace RescueTube.Core.Services;

public class PlaylistPresentationService
{
    private readonly IEnumerable<IPlatformPresentationHandler> _presentationHandlers;
    private readonly EntityMapper _mapper;
    private readonly VideoPresentationService _videoPresentationService;
    private readonly IDataUow _dataUow;

    public PlaylistPresentationService(IEnumerable<IPlatformPresentationHandler> presentationHandlers, EntityMapper mapper, VideoPresentationService videoPresentationService, IDataUow dataUow)
    {
        _presentationHandlers = presentationHandlers;
        _mapper = mapper;
        _videoPresentationService = videoPresentationService;
        _dataUow = dataUow;
    }

    public class PlaylistSearchParams
    {
        public string? Name { get; set; }
    }

    public async Task<PaginationResponse<List<PlaylistSimpleDto>>> SearchPlaylistsAsync(PlaylistSearchParams filter,
        IPaginationQuery paginationQuery, ClaimsPrincipal user)
    {
        var userId = user.GetUserIdIfExists();
        paginationQuery = paginationQuery.ToClamped();

        var playlistQuery = _dataUow.Playlists.SearchPlaylists(new IPlaylistSpecification.PlaylistSearchParams
            {
                Name = filter.Name,
                Author = null,
                UserId = userId,
            })
            .Paginate(paginationQuery)
            .Select(_mapper.ToPlaylistSimpleDto);

        var playlists = await playlistQuery.ToListAsync();

        MakePresentable(playlists);

        return new PaginationResponse<List<PlaylistSimpleDto>>
        {
            Result = playlists,
            PaginationResult = paginationQuery.ToPaginationResult(playlists.Count),
        };
    }

    public async Task<PlaylistDto?> GetPlaylistByIdAsync(Guid playlistId, CancellationToken ct)
    {
        var playlist = await _dataUow.Ctx.Playlists
            .Where(p => p.Id == playlistId)
            .Select(_mapper.ToPlaylistDto)
            .FirstOrDefaultAsync(ct);

        if (playlist is null)
        {
            return null;
        }

        MakePresentable(playlist);

        return playlist;
    }

    public async Task<PaginationResponse<PlaylistItemDto<VideoSimple>[]>> GetPlaylistItemsAsync(
        Guid playlistId, IPaginationQuery paginationQuery, CancellationToken ct)
    {
        var query = _dataUow.Ctx.PlaylistItems
            .Where(pi => pi.PlaylistId == playlistId);

        var playlistItems = await query
            .OrderBy(pi => pi.Position)
            .ThenBy(pi => pi.AddedAt)
            .ThenBy(pi => pi.Id)
            .Paginate(paginationQuery)
            .Select(_mapper.ToPlaylistItemDtoWithVideoSimple)
            .ToArrayAsync(ct);

        var count = await query.CountAsync(ct);

        foreach (var playlistItem in playlistItems)
        {
            _videoPresentationService.MakePresentable(playlistItem.Video);
        }

        return new PaginationResponse<PlaylistItemDto<VideoSimple>[]>
        {
            Result = playlistItems,
            PaginationResult = paginationQuery.ToPaginationResult(playlistItems.Length, count),
        };
    }

    private void MakePresentable(IEnumerable<IPlaylistDto> playlists)
    {
        foreach (var playlist in playlists)
        {
            MakePresentable(playlist);
        }
    }

    private void MakePresentable(IPlaylistDto playlist)
    {
        foreach (var presentationHandler in _presentationHandlers)
        {
            if (!presentationHandler.CanHandle(playlist)) continue;
            presentationHandler.Handle(playlist);
            break;
        }
    }
}