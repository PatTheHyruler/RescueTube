using RescueTube.Core.DTO.Enums;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Data.Specifications;

public interface IVideoSpecification
{
    public class VideoSearchParams
    {
        public EPlatform? Platform { get; set; }
        public string? Name { get; set; }
        public string? Author { get; set; }
        public ICollection<Guid>? CategoryIds { get; set; }
        public Guid? UserId { get; set; }
        public Guid? UserAuthorId { get; set; }
        public bool AccessAllowed { get; set; }
        public EVideoSortingOptions SortingOptions { get; set; }
        public bool Descending { get; set; }
    }

    public IQueryable<Video> SearchVideos(VideoSearchParams search);
}