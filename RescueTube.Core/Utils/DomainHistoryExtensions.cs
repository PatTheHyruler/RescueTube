using RescueTube.Domain.Contracts;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Utils;

public static class DomainHistoryExtensions
{
    public static CommentHistory ToHistory(this Comment comment, DateTimeOffset currentTime)
    {
        var history = comment.ToHistoryBase<Comment, CommentHistory>(currentTime);
        history.LastOfficialValidAt = GetMaxDateTimeOffset(comment.UpdatedAt, comment.CreatedAt);

        history.Content = comment.Content;
        history.CreatedAtVideoTimecode = comment.CreatedAtVideoTimecode;
        history.CreatedAt = comment.CreatedAt;
        history.UpdatedAt = comment.UpdatedAt;

        return history;
    }

    public static AuthorHistory ToHistory(this Author author, DateTimeOffset currentTime)
    {
        var history = author.ToHistoryBase<Author, AuthorHistory>(currentTime);
        history.LastOfficialValidAt = GetMaxDateTimeOffset(author.UpdatedAt, author.CreatedAt);

        history.UserName = author.UserName;
        history.DisplayName = author.DisplayName;
        history.CreatedAt = author.CreatedAt;
        history.UpdatedAt = author.UpdatedAt;

        return history;
    }

    private static THistoryEntity ToHistoryBase<TEntity, THistoryEntity>(this TEntity entity, DateTimeOffset currentTime)
        where THistoryEntity : IHistoryEntity<TEntity>, new()
        where TEntity : IIdDatabaseEntity
    {
        var history = new THistoryEntity
        {
            CurrentId = entity.Id,
            Current = entity,
            FirstNotValidAt = currentTime, // TODO: History rework
            LastValidAt = currentTime, // TODO: History rework
        };

        return history;
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