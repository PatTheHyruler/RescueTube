using System.Linq.Expressions;
using RescueTube.Core.DataFetches;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Data.Specifications;

public interface IDataFetchSpecification
{
    public Expression<Func<Author, bool>> ShouldFetchAuthorData(DataFetchJobDefinition jobDefinition);
    public Expression<Func<Playlist, bool>> ShouldFetchPlaylistData(DataFetchJobDefinition jobDefinition);
    public Expression<Func<Video, bool>> ShouldFetchVideoData(
        DataFetchJobDefinition jobDefinition, Expression<Func<Video, bool>> allowRegularFetchesPredicate);
}