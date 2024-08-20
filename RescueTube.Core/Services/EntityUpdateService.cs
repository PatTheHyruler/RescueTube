using System.Linq.Expressions;
using RescueTube.Core.Utils;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Base;
using RescueTube.Core.Data;
using RescueTube.Domain.Contracts;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Localization;
using RescueTube.Domain.Enums;

namespace RescueTube.Core.Services;

public class EntityUpdateService : BaseService
{
    private readonly IDataUow _dataUow;

    public EntityUpdateService(IServiceProvider services, ILogger<EntityUpdateService> logger, IDataUow dataUow) : base(
        services, logger)
    {
        _dataUow = dataUow;
    }

    public void UpdateVideo(Video video, Video newVideoData, bool isNew)
    {
        UpdateTranslations(video, v => v.Title, newVideoData.Title);
        UpdateTranslations(video, v => v.Description, newVideoData.Description);

        video.DefaultLanguage = newVideoData.DefaultLanguage ?? video.DefaultLanguage;
        video.DefaultAudioLanguage = newVideoData.DefaultAudioLanguage ?? video.DefaultAudioLanguage;

        video.Duration = newVideoData.Duration ?? video.Duration;

        video.VideoStatisticSnapshots ??= [];
        if (newVideoData.VideoStatisticSnapshots != null)
        {
            foreach (var videoStatisticSnapshot in newVideoData.VideoStatisticSnapshots.Where(s =>
                         s.ViewCount != null || s.CommentCount != null || s.LikeCount != null || s.DislikeCount != null
                     ))
            {
                videoStatisticSnapshot.Video = video;
                videoStatisticSnapshot.VideoId = video.Id;
                video.VideoStatisticSnapshots.Add(videoStatisticSnapshot);
                DbCtx.Add(videoStatisticSnapshot);
            }
        }

        if (isNew) // TODO: Properly handle update
        {
            video.Captions ??= newVideoData.Captions;
            video.VideoImages ??= newVideoData.VideoImages;

            video.VideoFiles ??= newVideoData.VideoFiles; // Not needed? At least currently not updated via this method
            // video.VideoAuthors ??= newVideoData.VideoAuthors;
        }

        video.VideoTags ??= [];
        var originalVideoTags = video.VideoTags
            .Where(t => t.ValidUntil == null)
            .ToList();
        if (newVideoData.VideoTags != null)
        {
            foreach (var originalVideoTag in originalVideoTags)
            {
                originalVideoTag.ValidUntil = DateTimeOffset.UtcNow;
            }

            foreach (var newVideoTag in newVideoData.VideoTags)
            {
                var originalVideoTag = originalVideoTags
                    .FirstOrDefault(t => t.Tag == newVideoTag.Tag);
                if (originalVideoTag != null)
                {
                    originalVideoTag.ValidUntil = null;
                }
                else
                {
                    video.VideoTags.Add(newVideoTag);
                    DbCtx.Add(newVideoTag);
                }
            }
        }

        video.InfoJsonPath = newVideoData.InfoJsonPath ?? video.InfoJsonPath;
        video.InfoJson = newVideoData.InfoJson ?? video.InfoJson;

        video.LastCommentsFetch = DateTimeUtils.GetLatest(video.LastCommentsFetch, newVideoData.LastCommentsFetch);

        video.LiveStatus = newVideoData.LiveStatus != ELiveStatus.None ? newVideoData.LiveStatus : video.LiveStatus;
        video.StreamId = newVideoData.StreamId ?? video.StreamId;
        video.LiveStreamStartedAt =
            DateTimeUtils.GetLatest(video.LiveStreamStartedAt, newVideoData.LiveStreamStartedAt);
        video.LiveStreamEndedAt = DateTimeUtils.GetLatest(video.LiveStreamEndedAt, newVideoData.LiveStreamEndedAt);

        UpdateBaseEntity(video, newVideoData, isNew);
        video.PublishedAt = DateTimeUtils.GetLatest(video.PublishedAt, newVideoData.PublishedAt);
        video.RecordedAt = DateTimeUtils.GetLatest(video.RecordedAt, newVideoData.RecordedAt);

        // TODO: VideoCategories
        // TODO: StatusChangeEvents?
    }

    public enum EImageUpdateOptions
    {
        NoUpdate,
        ExpireNonMatching,
        OnlyAdd,
    }

    public record UpdateAuthorOptions
    {
        public EImageUpdateOptions ImageUpdateOptions { get; set; } = EImageUpdateOptions.NoUpdate;
    }

    public void UpdateAuthor(Author author, Author newAuthorData, bool isNew, UpdateAuthorOptions options)
    {
        var changed = false;
        var history = author.ToHistory(newAuthorData);

        author.UserName = UpdateValueIgnoreNull(author.UserName, newAuthorData.UserName, ref changed);
        author.DisplayName = UpdateValueIgnoreNull(author.DisplayName, newAuthorData.DisplayName, ref changed);

        author.AuthorStatisticSnapshots ??= [];
        if (newAuthorData.AuthorStatisticSnapshots != null)
        {
            foreach (var authorStatisticSnapshot in newAuthorData.AuthorStatisticSnapshots.Where(s =>
                         s.FollowerCount != null || s.PaidFollowerCount != null
                     ))
            {
                authorStatisticSnapshot.Author = author;
                authorStatisticSnapshot.AuthorId = author.Id;
                author.AuthorStatisticSnapshots.Add(authorStatisticSnapshot);
                DbCtx.Add(authorStatisticSnapshot);
            }
        }

        UpdateTranslations(author, a => a.Bio, newAuthorData.Bio);
        UpdateAuthorImages(author, newAuthorData, isNew, options);

        // Skipping VideoAuthors, TODO???

        author.ArchivalSettings = newAuthorData.ArchivalSettings ?? author.ArchivalSettings;

        UpdateBaseEntity(author, newAuthorData, isNew);

        if (changed)
        {
            _dataUow.Ctx.AuthorHistories.Add(history);
        }
    }

    private void UpdateAuthorImages(Author author, Author newAuthorData, bool isNew, UpdateAuthorOptions options)
    {
        if (newAuthorData.AuthorImages is { Count: > 0 } && options.ImageUpdateOptions != EImageUpdateOptions.NoUpdate)
        {
            if (isNew)
            {
                author.AuthorImages ??= new List<AuthorImage>();
            }

            if (author.AuthorImages == null)
            {
                throw new Exception(
                    $"Can't update {nameof(author.AuthorImages)} for author {author.Id} ({author.IdOnPlatform}) from {newAuthorData.Id}, navigation not loaded");
            }

            var currentTime = DateTimeOffset.UtcNow;
            var validAuthorImages = new List<AuthorImage>();
            foreach (var newAuthorImage in newAuthorData.AuthorImages)
            {
                var existingAuthorImage = author.AuthorImages.FirstOrDefault(ai =>
                    ai.ValidUntil == null &&
                    ai.Image.AssertNotNull(ai.Id.ToString()).Url == newAuthorImage.Image!.Url.AssertNotNull());
                if (existingAuthorImage != null)
                {
                    existingAuthorImage.LastFetched = DateTimeOffset.UtcNow;
                    validAuthorImages.Add(existingAuthorImage);
                }
                else
                {
                    newAuthorImage.ValidSince ??= currentTime;
                    validAuthorImages.Add(newAuthorImage);
                    author.AuthorImages.Add(newAuthorImage);
                    DbCtx.Add(newAuthorImage);
                }
            }

            if (options.ImageUpdateOptions == EImageUpdateOptions.ExpireNonMatching)
            {
                foreach (var authorImage in author.AuthorImages.Where(ai =>
                             ai.ValidUntil == null
                             && !validAuthorImages.Contains(ai)))
                {
                    authorImage.ValidUntil = currentTime;
                }
            }
        }
    }

    public void UpdateTranslations<TEntity>(TEntity entity,
        Expression<Func<TEntity, TextTranslationKey?>> translationsPropertyExpression,
        TextTranslationKey? newTranslationKey)
    {
        if (translationsPropertyExpression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Invalid expression type", nameof(translationsPropertyExpression));
        }

        var getter = translationsPropertyExpression.Compile();
        var existingTranslationKey = getter(entity);
        if (existingTranslationKey == null)
        {
            existingTranslationKey = new TextTranslationKey();
            var thisParameter = Expression.Parameter(typeof(TEntity), "$this");
            var valueParameter = Expression.Parameter(typeof(TextTranslationKey), "value");
            var assign = Expression.Assign(Expression.MakeMemberAccess(thisParameter, memberExpression.Member),
                valueParameter);
            var setter = Expression
                .Lambda<Action<TEntity, TextTranslationKey>>(assign, thisParameter, valueParameter)
                .Compile();
            setter(entity, existingTranslationKey);
            DbCtx.Add(existingTranslationKey);
        }

        existingTranslationKey.Translations ??= new List<TextTranslation>();

        var newTranslations = newTranslationKey?.Translations;
        if (newTranslations == null) return;
        foreach (var newTranslation in newTranslations)
        {
            var existingTranslation = existingTranslationKey.Translations
                .FirstOrDefault(t => t.Culture == newTranslation.Culture);
            if (newTranslation.Content == existingTranslation?.Content)
            {
                continue;
            }

            existingTranslationKey.Translations.Add(newTranslation);
            DbCtx.Add(newTranslation);
            if (existingTranslation != null)
            {
                var validityChangeTime = newTranslation.ValidSince ?? DateTimeOffset.UtcNow;
                existingTranslation.ValidUntil = validityChangeTime;
                newTranslation.ValidSince ??= validityChangeTime;
            }
        }
    }

    public void UpdateComment(Comment comment, Comment newCommentData, bool isNew)
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
            DbCtx.Add(newStats);
        }

        comment.AuthorIsCreator ??= newCommentData.AuthorIsCreator;

        comment.CreatedAtVideoTimecode ??= newCommentData.CreatedAtVideoTimecode;

        comment.OrderIndex = newCommentData.OrderIndex;

        UpdateBaseEntity(comment, newCommentData, isNew);

        if (changed)
        {
            _dataUow.Ctx.CommentHistories.Add(commentHistory);
        }
    }

    private void UpdateBaseEntity<TEntity>(TEntity entity, TEntity newEntityData, bool isNew)
        where TEntity : IMainArchiveEntity
    {
        entity.CreatedAt = DateTimeUtils.GetLatest(entity.CreatedAt, newEntityData.CreatedAt);
        entity.UpdatedAt = DateTimeUtils.GetLatest(entity.UpdatedAt, newEntityData.UpdatedAt);

        if (!isNew &&
            newEntityData.PrivacyStatus != entity.PrivacyStatus)
        {
            var statusChangeEvent = entity switch
            {
                Video video => new StatusChangeEvent(video, newEntityData.PrivacyStatus,
                    newEntityData.UpdatedAt),
                Playlist playlist => new StatusChangeEvent(playlist, newEntityData.PrivacyStatus,
                    newEntityData.UpdatedAt),
                Author author => new StatusChangeEvent(author, newEntityData.PrivacyStatus,
                    newEntityData.UpdatedAt),
                _ => null
            };

            if (statusChangeEvent != null)
            {
                ServiceUow.StatusChangeService.Push(statusChangeEvent);
            }
        }

        if (isNew)
        {
            entity.Platform = newEntityData.Platform;
            entity.IdOnPlatform = newEntityData.IdOnPlatform;
        }

        entity.PrivacyStatusOnPlatform ??= newEntityData.PrivacyStatusOnPlatform;
        if (isNew)
        {
            entity.PrivacyStatus =
                newEntityData
                    .PrivacyStatus; // TODO: Add way for updating this. Archive-specific property, will only be updated manually, not via datafetches, so shouldn't be updated here.
        }

        if (newEntityData.DataFetches != null)
        {
            if (entity.DataFetches == null)
            {
                entity.DataFetches = newEntityData.DataFetches;
                foreach (var dataFetch in newEntityData.DataFetches)
                {
                    DbCtx.Add(dataFetch);
                }
            }
            else
            {
                foreach (var dataFetch in newEntityData.DataFetches)
                {
                    entity.DataFetches.Add(dataFetch);
                    DbCtx.Add(dataFetch);
                }
            }
        }

        entity.AddedToArchiveAt = entity.AddedToArchiveAt == default
            ? newEntityData.AddedToArchiveAt
            : entity.AddedToArchiveAt;
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