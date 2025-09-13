using System.Linq.Expressions;
using RescueTube.Core.DTO.Enums;
using RescueTube.Core.DTO.Videos;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Data.Specifications;

public interface IVideoSpecification
{
    public class VideoSearchParams
    {
        public VideoSearchFilter? Filter { get; init; }
        public Guid? UserId { get; init; }
        public EVideoSortingOptions SortingOptions { get; init; }
        public bool Descending { get; init; }
    }

    public Expression<Func<Video, bool>> FilterVideos(VideoSearchFilter filter);
    public IQueryable<Video> SearchVideos(VideoSearchParams search);
}