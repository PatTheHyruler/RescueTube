using RescueTube.Core.Data;
using RescueTube.Core.Data.Repositories;

namespace RescueTube.DAL.EF;

public class DataUow : IDataUow
{
    public DataUow(AppDbContext ctx, IVideoRepository videoRepo)
    {
        _ctx = ctx;
        VideoRepo = videoRepo;
    }

    public IVideoRepository VideoRepo { get; }
    public IAppDbContext Ctx => _ctx;
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