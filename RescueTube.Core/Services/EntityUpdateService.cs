using System.Linq.Expressions;
using RescueTube.Core.Utils;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Base;
using RescueTube.Core.Data;
using RescueTube.Domain.Contracts;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Localization;

namespace RescueTube.Core.Services;

public class EntityUpdateService : BaseService
{
    private readonly IDataUow _dataUow;

    public EntityUpdateService(IServiceProvider services, ILogger<EntityUpdateService> logger, IDataUow dataUow) : base(
        services, logger)
    {
        _dataUow = dataUow;
    }

    public async Task UpdateVideo(Video video, Video newVideoData, bool isNew)
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
                DbCtx.VideoStatisticSnapshots.Add(videoStatisticSnapshot);
            }
        }

        video.Captions ??= newVideoData.Captions; // TODO: Proper captions update
        video.VideoImages ??= newVideoData.VideoImages; // TODO: Proper images update

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
                }
            }
        }

        video.VideoFiles ??= newVideoData.VideoFiles;

        video.FailedDownloadAttempts = newVideoData.FailedDownloadAttempts;
        video.FailedAuthorFetches = newVideoData.FailedAuthorFetches;
        video.InfoJsonPath = newVideoData.InfoJsonPath ?? video.InfoJsonPath;
        video.InfoJson = newVideoData.InfoJson ?? video.InfoJson;

        video.LastCommentsFetch = DateTimeUtils.GetLatest(video.LastCommentsFetch, newVideoData.LastCommentsFetch);

        video.IsLiveStreamRecording = newVideoData.IsLiveStreamRecording ?? video.IsLiveStreamRecording;
        video.StreamId = newVideoData.StreamId ?? video.StreamId;
        video.LiveStreamStartedAt =
            DateTimeUtils.GetLatest(video.LiveStreamStartedAt, newVideoData.LiveStreamStartedAt);
        video.LiveStreamEndedAt = DateTimeUtils.GetLatest(video.LiveStreamEndedAt, newVideoData.LiveStreamEndedAt);

        await UpdateBaseEntity(video, newVideoData, isNew);
        video.PublishedAt = DateTimeUtils.GetLatest(video.PublishedAt, newVideoData.PublishedAt);
        video.RecordedAt = DateTimeUtils.GetLatest(video.RecordedAt, newVideoData.RecordedAt);

        video.VideoAuthors ??= newVideoData.VideoAuthors;

        // TODO: VideoCategories
        // TODO: StatusChangeEvents?
    }

    public static void UpdateTranslations<TEntity>(TEntity entity,
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
            if (existingTranslation != null)
            {
                var validityChangeTime = newTranslation.ValidSince ?? DateTimeOffset.UtcNow;
                existingTranslation.ValidUntil = validityChangeTime;
                newTranslation.ValidSince ??= validityChangeTime;
            }
        }
    }

    public async Task UpdateComment(Comment comment, Comment newCommentData, bool isNew)
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
            DataUow.Ctx.CommentStatisticSnapshots.Add(newStats);
        }

        comment.AuthorIsCreator ??= newCommentData.AuthorIsCreator;

        comment.CreatedAtVideoTimecode ??= newCommentData.CreatedAtVideoTimecode;

        comment.OrderIndex = newCommentData.OrderIndex;

        await UpdateBaseEntity(comment, newCommentData, isNew);

        if (changed)
        {
            _dataUow.Ctx.CommentHistories.Add(commentHistory);
        }
    }

    private async Task UpdateBaseEntity<TEntity>(TEntity entity, TEntity newEntityData, bool isNew)
        where TEntity : IMainArchiveEntity
    {
        entity.CreatedAt = DateTimeUtils.GetLatest(entity.CreatedAt, newEntityData.CreatedAt);
        entity.UpdatedAt = DateTimeUtils.GetLatest(entity.UpdatedAt, newEntityData.UpdatedAt);

        if (!isNew &&
            (newEntityData.PrivacyStatus != entity.PrivacyStatus
             || newEntityData.IsAvailable != entity.IsAvailable))
        {
            var statusChangeEvent = entity switch
            {
                Video video => new StatusChangeEvent(video, newEntityData.PrivacyStatus, newEntityData.IsAvailable,
                    newEntityData.UpdatedAt),
                Playlist playlist => new StatusChangeEvent(playlist, newEntityData.PrivacyStatus,
                    newEntityData.IsAvailable,
                    newEntityData.UpdatedAt),
                Author author => new StatusChangeEvent(author, newEntityData.PrivacyStatus, newEntityData.IsAvailable,
                    newEntityData.UpdatedAt),
                _ => null
            };

            if (statusChangeEvent != null)
            {
                await ServiceUow.StatusChangeService.Push(statusChangeEvent);
            }
        }

        if (isNew)
        {
            entity.Platform = newEntityData.Platform;
        }

        entity.PrivacyStatusOnPlatform ??= newEntityData.PrivacyStatusOnPlatform;
        entity.IsAvailable = newEntityData.IsAvailable;
        entity.PrivacyStatus =
            newEntityData
                .PrivacyStatus; // TODO: actually when updating from a fetch, this should be left at original. Handle outside update function?

        entity.LastFetchOfficial = DateTimeUtils.GetLatest(entity.LastFetchOfficial, newEntityData.LastFetchOfficial);
        entity.LastSuccessfulFetchOfficial =
            DateTimeUtils.GetLatest(entity.LastSuccessfulFetchOfficial, newEntityData.LastSuccessfulFetchOfficial);
        entity.LastFetchUnofficial =
            DateTimeUtils.GetLatest(entity.LastFetchUnofficial, newEntityData.LastFetchUnofficial);
        entity.LastSuccessfulFetchUnofficial =
            DateTimeUtils.GetLatest(entity.LastSuccessfulFetchUnofficial, newEntityData.LastSuccessfulFetchUnofficial);

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