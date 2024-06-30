using Microsoft.EntityFrameworkCore;
using RescueTube.Core.Data.Specifications;
using RescueTube.Domain.Entities;

namespace RescueTube.DAL.EF.Specifications;

public class PlaylistSpecification : BaseDbService, IPlaylistSpecification
{
    public PlaylistSpecification(IServiceProvider services) : base(services)
    {
    }

    public IQueryable<Playlist> SearchPlaylists(IPlaylistSpecification.PlaylistSearchParams search)
    {
        IQueryable<Playlist> query = Ctx.Playlists;

        if (!string.IsNullOrEmpty(search.Name))
        {
            var nameQuery = '%' + Utils.EscapeWildcards(search.Name) + '%';
            query = query.Where(e => e.Title!.Translations!
                .Any(t => Microsoft.EntityFrameworkCore.EF.Functions
                    .ILike(t.Content, nameQuery, "\\")));
        }

        if (!string.IsNullOrEmpty(search.Author))
        {
            var authorQuery = '%' + Utils.EscapeWildcards(search.Author) + '%';
            query = query.Where(e =>
                Microsoft.EntityFrameworkCore.EF.Functions.ILike(
                    e.Creator!.UserName + e.Creator!.DisplayName,
                    authorQuery));
        }

        if (!search.AccessAllowed)
        {
            query = query.Where(DataUow.Permissions
                .IsUserAllowedToAccessPlaylistOrPlaylistIsPublic(search.UserId, true));
        }

        return query;
    }
}