using RescueTube.Core.Data;
using RescueTube.Core.Data.Specifications;

namespace RescueTube.DAL.EF;

public class DataUow : IDataUow
{
    public DataUow(AppDbContext ctx, IVideoSpecification videos, IPlaylistSpecification playlists,
        IPermissionSpecification permissions, IImageSpecification images, IDataFetchSpecification dataFetches)
    {
        _ctx = ctx;
        Videos = videos;
        Playlists = playlists;
        Permissions = permissions;
        Images = images;
        DataFetches = dataFetches;
    }

    public IVideoSpecification Videos { get; }
    public IPlaylistSpecification Playlists { get; }
    public IPermissionSpecification Permissions { get; }
    public IImageSpecification Images { get; }
    public IDataFetchSpecification DataFetches { get; }

    public AppDbContext Ctx => _ctx;
    private readonly AppDbContext _ctx;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _ctx.SaveChangesAsync(cancellationToken);

    public void RegisterSavedChangesCallbackRunOnce(Action callback)
    {
        RegisterSavedChangesCallbackRunOnce(_ => callback());
    }

    private void RegisterSavedChangesCallbackRunOnce(Action<object?> callback)
    {
        _ctx.SavedChanges += OnSavedChanges;
        return;

        void OnSavedChanges(object? sender, EventArgs savedEventArgs)
        {
            _ctx.SavedChanges -= OnSavedChanges;

            callback(sender);
        }
    }
}