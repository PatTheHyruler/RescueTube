using System.Linq.Expressions;
using RescueTube.Core.DataFetches;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Data.Specifications;

public interface IDataFetchSpecification
{
    public Expression<Func<DataFetch, Video, bool>> IsVideoDataFetch { get; }
    public Expression<Func<DataFetch, bool>> IsOngoing(DateTimeOffset currentTime);
    public Expression<Func<Author, bool>> ShouldFetchAuthorData(DataFetchDefinition dataFetchDefinition, DataFetchJobSettings dataFetchJobSettings);
    public Expression<Func<Playlist, bool>> ShouldFetchPlaylistData(DataFetchDefinition dataFetchDefinition, DataFetchJobSettings dataFetchJobSettings);
    public Expression<Func<Video, bool>> ShouldFetchVideoData(
        DataFetchDefinition dataFetchDefinition, DataFetchJobSettings dataFetchJobSettings, Expression<Func<Video, bool>> allowRegularFetchesPredicate);
}