using RescueTube.Domain.Entities;

namespace RescueTube.Core.Data.Specifications;

public interface IPlaylistSpecification
{
    public class PlaylistSearchParams
    {
        public string? Name { get; set; }
        public string? Author { get; set; }
        public Guid? UserId { get; set; }
        public bool AccessAllowed { get; set; }
    }

    public IQueryable<Playlist> SearchPlaylists(PlaylistSearchParams search);
}