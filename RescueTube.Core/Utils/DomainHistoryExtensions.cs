using RescueTube.Domain.Contracts;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Utils;

public static class DomainHistoryExtensions
{
    public static CommentHistory ToHistory(this Comment comment, Comment newComment)
    {
        var history = comment.ToHistoryBase<Comment, CommentHistory>(newComment);
        history.LastOfficialValidAt = GetMaxDateTimeOffset(comment.UpdatedAt, comment.CreatedAt);

        history.Content = comment.Content;
        history.CreatedAtVideoTimecode = comment.CreatedAtVideoTimecode;
        history.CreatedAt = comment.CreatedAt;
        history.UpdatedAt = comment.UpdatedAt;

        return history;
    }

    private static THistoryEntity ToHistoryBase<TEntity, THistoryEntity>(this TEntity entity,
        TEntity newEntity)
        where THistoryEntity : IHistoryEntity<TEntity>, new()
        where TEntity : IIdDatabaseEntity, IFetchable
    {
        var history = new THistoryEntity
        {
            CurrentId = entity.Id,
            Current = entity,
            FirstNotValidAt = GetMaxDateTimeOffset(
                newEntity.LastFetchOfficial, newEntity.LastFetchUnofficial
                ) ?? DateTimeOffset.UtcNow,
            LastValidAt = GetMaxDateTimeOffset(entity.LastSuccessfulFetchUnofficial, entity.LastSuccessfulFetchOfficial)
                ?? DateTimeOffset.UtcNow,
        };

        return history;
    }

    private static DateTimeOffset? GetMaxDateTimeOffset(params DateTimeOffset?[] values)
    {
        return values.Aggregate<DateTimeOffset?, DateTimeOffset?>(null, GetMaxDateTimeOffset);
    }

    private static DateTimeOffset? GetMaxDateTimeOffset(DateTimeOffset? existing, DateTimeOffset? replacement)
    {
        if (existing == null)
        {
            return replacement;
        }

        if (replacement == null)
        {
            return existing;
        }

        if (existing > replacement)
        {
            return existing;
        }

        return replacement;
    }
}