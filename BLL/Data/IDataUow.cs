using BLL.Data.Repositories;

namespace BLL.Data;

public interface IDataUow
{
    public IVideoRepository VideoRepo { get; }

    public IAppDbContext Ctx { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    void RegisterSavedChangesCallbackRunOnce(Action callback);
}