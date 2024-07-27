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

    public static AuthorHistory ToHistory(this Author author, Author newAuthor)
    {
        var history = author.ToHistoryBase<Author, AuthorHistory>(newAuthor);
        history.LastOfficialValidAt = GetMaxDateTimeOffset(author.UpdatedAt, author.CreatedAt);

        history.UserName = author.UserName;
        history.DisplayName = author.DisplayName;
        history.CreatedAt = author.CreatedAt;
        history.UpdatedAt = author.UpdatedAt;

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
            FirstNotValidAt = newEntity.DataFetches?
                .Where(x => x is { ShouldAffectValidity: true, Success: true })
                .Select(x => x.OccurredAt)
                .Min() ?? DateTimeOffset.UtcNow,
            LastValidAt = entity.DataFetches?
                .Where(x => x is { ShouldAffectValidity: true, Success: true })
                .Select(x => x.OccurredAt)
                .Max() ?? DateTimeOffset.UtcNow,
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