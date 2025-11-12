using RescueTube.Core.Data;
using RescueTube.Core.Data.Specifications;

namespace RescueTube.DAL.EF;

public class DataUow : IDataUow
{
    public DataUow(AppDbContext ctx, IVideoSpecification videos, IPlaylistSpecification playlists,
        IImageSpecification images, IDataFetchSpecification dataFetches, IAuthorSpecification authors)
    {
        _ctx = ctx;
        Videos = videos;
        Playlists = playlists;
        Images = images;
        DataFetches = dataFetches;
        Authors = authors;
    }

    public IVideoSpecification Videos { get; }
    public IAuthorSpecification Authors { get; }
    public IPlaylistSpecification Playlists { get; }
    public IImageSpecification Images { get; }
    public IDataFetchSpecification DataFetches { get; }

    public AppDbContext Ctx => _ctx;
    private readonly AppDbContext _ctx;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _ctx.SaveChangesAsync(cancellationToken);
}