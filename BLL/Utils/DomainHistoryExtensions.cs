using Contracts.Domain;
using Domain.Contracts;
using Domain.Entities;

namespace BLL.Utils;

public static class DomainHistoryExtensions
{
    public static CommentHistory ToHistory(this Comment comment, Comment newComment)
    {
        var history = comment.ToHistoryBase<Comment, CommentHistory>(newComment);
        history.LastOfficialValidAt = GetMaxDateTime(comment.DeletedAt, comment.UpdatedAt, comment.CreatedAt);

        history.Content = comment.Content;
        history.CreatedAtVideoTimecode = comment.CreatedAtVideoTimecode;
        history.CreatedAt = comment.CreatedAt;
        history.DeletedAt = comment.DeletedAt;
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
            FirstNotValidAt = GetMaxDateTime(
                newEntity.LastFetchOfficial, newEntity.LastFetchUnofficial
                ) ?? DateTime.UtcNow,
            LastValidAt = GetMaxDateTime(entity.LastSuccessfulFetchUnofficial, entity.LastSuccessfulFetchOfficial)
                ?? DateTime.UtcNow,
        };

        return history;
    }

    private static DateTime? GetMaxDateTime(params DateTime?[] values)
    {
        return values.Aggregate<DateTime?, DateTime?>(null, GetMaxDateTime);
    }

    private static DateTime? GetMaxDateTime(DateTime? existing, DateTime? replacement)
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