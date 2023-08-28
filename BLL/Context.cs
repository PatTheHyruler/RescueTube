using DAL.EF.DbContexts;
using Domain.Enums;

namespace BLL;

public static class Context
{
    public static event EventHandler<PlatformEntityAddedEventArgs>? VideoAdded;
    public static event EventHandler<PlatformEntityAddedEventArgs>? AuthorAdded;

    public static void RegisterVideoAddedCallback(this AbstractAppDbContext dbContext,
        PlatformEntityAddedEventArgs args) =>
        dbContext.RegisterSavedChangesCallbackRunOnce(VideoAdded, args);

    public static void RegisterAuthorAddedCallback(this AbstractAppDbContext dbContext,
        PlatformEntityAddedEventArgs args) =>
        dbContext.RegisterSavedChangesCallbackRunOnce(AuthorAdded, args);

    private static void RegisterSavedChangesCallbackRunOnce<TEventArgs>(this AbstractAppDbContext dbContext,
        EventHandler<TEventArgs>? eventHandler, TEventArgs args)
    {
        void OnSavedChanges(object? sender, EventArgs savedEventArgs)
        {
            dbContext.SavedChanges -= OnSavedChanges;

            eventHandler?.Invoke(sender, args);
        }

        dbContext.SavedChanges += OnSavedChanges;
    }
}

public class PlatformEntityAddedEventArgs : EventArgs
{
    public PlatformEntityAddedEventArgs(Guid id, EPlatform platform, string idOnPlatform)
    {
        Platform = platform;
        IdOnPlatform = idOnPlatform;
        Id = id;
    }

    public Guid Id { get; set; }
    public EPlatform Platform { get; set; }
    public string IdOnPlatform { get; set; }
}