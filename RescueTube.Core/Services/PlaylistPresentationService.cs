using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Base;
using RescueTube.Core.Contracts;
using RescueTube.Core.Data.Mappers;
using RescueTube.Core.Data.Pagination;
using RescueTube.Core.Data.Specifications;
using RescueTube.Core.DTO.Entities;
using RescueTube.Core.Identity.Services;
using RescueTube.Core.Utils.Pagination;

namespace RescueTube.Core.Services;

public class PlaylistPresentationService : BaseService
{
    private readonly IEnumerable<IPlatformPresentationHandler> _presentationHandlers;
    private readonly EntityMapper _mapper;

    public PlaylistPresentationService(IServiceProvider services, ILogger<PlaylistPresentationService> logger,
        IEnumerable<IPlatformPresentationHandler> presentationHandlers, EntityMapper mapper) : base(services, logger)
    {
        _presentationHandlers = presentationHandlers;
        _mapper = mapper;
    }

    public class PlaylistSearchParams
    {
        public string? Name { get; set; }
    }

    public async Task<PaginationResponse<List<PlaylistDto>>> SearchPlaylists(PlaylistSearchParams search,
        IPaginationQuery paginationQuery, ClaimsPrincipal user)
    {
        var userId = user.GetUserIdIfExists();
        var accessAllowed = AuthorizationService.IsAllowedToAccessAnyContentByRole(user);
        paginationQuery = paginationQuery.ToClamped();

        var playlistQuery = DataUow.Playlists.SearchPlaylists(new IPlaylistSpecification.PlaylistSearchParams
            {
                Name = search.Name,
                Author = null,
                UserId = userId,
                AccessAllowed = accessAllowed,
            })
            .Paginate(paginationQuery)
            .Select(_mapper.ToPlaylistDto);

        var playlists = await playlistQuery.ToListAsync();

        MakePresentable(playlists);

        return new PaginationResponse<List<PlaylistDto>>
        {
            Result = playlists,
            PaginationResult = paginationQuery.ToPaginationResult(playlists.Count),
        };
    }

    private void MakePresentable(IEnumerable<PlaylistDto> playlists)
    {
        foreach (var playlist in playlists)
        {
            MakePresentable(playlist);
        }
    }

    private void MakePresentable(PlaylistDto playlist)
    {
        foreach (var presentationHandler in _presentationHandlers)
        {
            if (!presentationHandler.CanHandle(playlist)) continue;
            presentationHandler.Handle(playlist);
            break;
        }
    }
}