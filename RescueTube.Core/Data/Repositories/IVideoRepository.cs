using RescueTube.Core.DTO.Enums;
using RescueTube.Core.Utils.Pagination;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Data.Repositories;

public interface IVideoRepository
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
        public required IPaginationQuery PaginationQuery { get; set; }
        public EVideoSortingOptions SortingOptions { get; set; }
        public bool Descending { get; set; }
    }

    public IQueryable<Video> SearchVideos(VideoSearchParams search);

    public IQueryable<Video> WhereUserIsAllowedToAccessVideoOrVideoIsPublic(IQueryable<Video> query, Guid? userId);
}