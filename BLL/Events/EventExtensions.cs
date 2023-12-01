using Microsoft.EntityFrameworkCore;

namespace BLL.Events;

public static class EventExtensions
{
    public static void RegisterSavedChangesCallbackRunOnce(this DbContext dbContext, Action callback)
    {
        dbContext.RegisterSavedChangesCallbackRunOnce(_ => callback());
    }

    private static void RegisterSavedChangesCallbackRunOnce(this DbContext dbContext, Action<object?> callback)
    {
        dbContext.SavedChanges += OnSavedChanges;
        return;

        void OnSavedChanges(object? sender, EventArgs savedEventArgs)
        {
            dbContext.SavedChanges -= OnSavedChanges;

            callback(sender);
        }
    }
}