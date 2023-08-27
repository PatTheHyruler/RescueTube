using DAL.EF.DbContexts;
using Domain.Enums;

namespace BLL;

public static class AppContext
{
    public static event EventHandler<PlatformEntityAddedEventArgs>? VideoAdded;
    public static event EventHandler<PlatformEntityAddedEventArgs>? AuthorAdded; 

    public static void RegisterVideoAddedCallback(this AbstractAppDbContext dbContext, PlatformEntityAddedEventArgs args)
    {
        void OnSavedChanges(object? sender, EventArgs savedEventArgs)
        {
            dbContext.SavedChanges -= OnSavedChanges;

            VideoAdded?.Invoke(sender, args);
        }

        dbContext.SavedChanges += OnSavedChanges;
    }

    public static void RegisterAuthorAddedCallback(this AbstractAppDbContext dbContext,
        PlatformEntityAddedEventArgs args)
    {
        void OnSavedChanges(object? sender, EventArgs savedEventArgs)
        {
            dbContext.SavedChanges -= OnSavedChanges;
            
            AuthorAdded?.Invoke(sender, args);
        }

        dbContext.SavedChanges += OnSavedChanges;
    }
}

public class PlatformEntityAddedEventArgs : EventArgs
{
    public PlatformEntityAddedEventArgs(EPlatform platform, string idOnPlatform)
    {
        Platform = platform;
        IdOnPlatform = idOnPlatform;
    }
    
    public EPlatform Platform { get; set; }
    public string IdOnPlatform { get; set; }
}