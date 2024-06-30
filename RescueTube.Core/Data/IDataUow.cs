using RescueTube.Core.Data.Specifications;

namespace RescueTube.Core.Data;

public interface IDataUow
{
    public IVideoSpecification Videos { get; }
    public IPlaylistSpecification Playlists { get; }
    public IPermissionSpecification Permissions { get; }

    public IAppDbContext Ctx { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    void RegisterSavedChangesCallbackRunOnce(Action callback);
}