using BLL.Base;
using BLL.Utils;
using DAL.EF.DbContexts;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class EntityUpdateService : BaseService
{
    private readonly AppDbContext _dbCtx;
    
    public EntityUpdateService(IServiceProvider services, ILogger<EntityUpdateService> logger, AppDbContext dbCtx) : base(services, logger)
    {
        _dbCtx = dbCtx;
    }

    public async Task UpdateComment(Comment comment, Comment newCommentData)
    {
        var changed = false;
        var commentHistory = comment.ToHistory(newCommentData);

        comment.Content = UpdateValueIgnoreNull(comment.Content, newCommentData.Content, ref changed);

        var newStats = newCommentData.CommentStatisticSnapshots?.SingleOrDefault();
        if (newStats == null)
        {
            Logger.LogWarning("New data for comment {Id} didn't have any associated statistics", comment.Id);
        }
        else
        {
            if (comment.CommentStatisticSnapshots == null)
            {
                throw new ArgumentException("Expected comment with statistic snapshots present", nameof(comment));
            }

            // Overriding values just in case
            newStats.Comment = comment;
            newStats.CommentId = comment.Id;

            comment.CommentStatisticSnapshots.Add(newStats);
            Ctx.CommentStatisticSnapshots.Add(newStats);
        }

        comment.AuthorIsCreator ??= newCommentData.AuthorIsCreator;

        comment.CreatedAtVideoTimecode ??= newCommentData.CreatedAtVideoTimecode;
        comment.DeletedAt ??= newCommentData.DeletedAt;

        comment.OrderIndex = newCommentData.OrderIndex;

        await UpdateBaseEntity(comment, newCommentData);

        if (changed)
        {
            _dbCtx.CommentHistories.Add(commentHistory);
        }
    }
    
    private async Task UpdateBaseEntity<TEntity>(TEntity entity, TEntity newEntityData)
        where TEntity : IMainArchiveEntity
    {
        entity.CreatedAt ??= newEntityData.CreatedAt;
        entity.UpdatedAt ??= newEntityData.UpdatedAt ?? DateTime.UtcNow;

        var statusChangeEvent = entity switch
        {
            Video video => new StatusChangeEvent(video, newEntityData.PrivacyStatus, newEntityData.IsAvailable,
                newEntityData.UpdatedAt),
            // Playlist playlist => new StatusChangeEvent(playlist, newEntityData.PrivacyStatus, newEntityData.IsAvailable,
            //     newEntityData.UpdatedAt),
            Author author => new StatusChangeEvent(author, newEntityData.PrivacyStatus, newEntityData.IsAvailable,
                newEntityData.UpdatedAt),
            _ => null
        };

        if (statusChangeEvent != null)
        {
            await ServiceUow.StatusChangeService.Push(statusChangeEvent);
        }

        entity.PrivacyStatusOnPlatform ??= newEntityData.PrivacyStatusOnPlatform;
        entity.IsAvailable = newEntityData.IsAvailable;
        entity.PrivacyStatus = newEntityData.PrivacyStatus;

        entity.LastFetchOfficial = newEntityData.LastFetchOfficial ?? entity.LastFetchOfficial;
        entity.LastSuccessfulFetchOfficial =
            newEntityData.LastSuccessfulFetchOfficial ?? entity.LastSuccessfulFetchOfficial;
        entity.LastFetchUnofficial = newEntityData.LastFetchUnofficial ?? entity.LastFetchUnofficial;
        entity.LastSuccessfulFetchUnofficial =
            newEntityData.LastSuccessfulFetchUnofficial ?? entity.LastSuccessfulFetchUnofficial;
    }

    private static void UpdateChanged(ref bool changed, bool addition)
    {
        changed = changed || addition;
    }

    private static T? UpdateValueIgnoreNull<T>(T? oldValue, T? newValue, ref bool changed,
        Func<T, T, bool> customChangedFunc)
    {
        if (newValue == null) return oldValue;
        if (oldValue == null) return newValue;
        UpdateChanged(ref changed, customChangedFunc(oldValue, newValue));
        return newValue;
    }

    private static T? UpdateValueIgnoreNull<T>(T? oldValue, T? newValue, ref bool changed,
        CustomUpdateFunc<T>? customUpdateFunc = null)
    {
        if (newValue == null) return oldValue;
        if (oldValue == null) return newValue;
        if (customUpdateFunc != null) return customUpdateFunc(oldValue, newValue, ref changed);
        UpdateChanged(ref changed, !oldValue.Equals(newValue));
        return newValue;
    }
}

internal delegate T CustomUpdateFunc<T>(T oldValue, T newValue, ref bool changed);