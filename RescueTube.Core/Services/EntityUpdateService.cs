using System.Linq.Expressions;
using RescueTube.Core.Utils;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Base;
using RescueTube.Core.Data;
using RescueTube.Core.Utils.ExpressionUtils;
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

    public void UpdateVideo(Video video, Video newVideoData, bool isNew, EImageUpdateOptions imageUpdateOptions)
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

        UpdateEntityImages(video, v => v.VideoImages, newVideoData.VideoImages, isNew, imageUpdateOptions);

        if (isNew) // TODO: Properly handle update
        {
            video.Captions ??= newVideoData.Captions;

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

    public void UpdatePlaylist(Playlist playlist, Playlist newPlaylistData, bool isNew,
        EImageUpdateOptions imageUpdateOptions)
    {
        UpdateTranslations(playlist, p => p.Title, newPlaylistData.Title);
        UpdateTranslations(playlist, p => p.Description, newPlaylistData.Description);

        playlist.PlaylistStatisticSnapshots ??= [];
        if (newPlaylistData.PlaylistStatisticSnapshots != null)
        {
            foreach (var playlistStatisticSnapshot in newPlaylistData.PlaylistStatisticSnapshots.Where(s =>
                         s.ViewCount != null || s.CommentCount != null || s.LikeCount != null ||
                         s.DislikeCount != null))
            {
                playlistStatisticSnapshot.Playlist = playlist;
                playlistStatisticSnapshot.PlaylistId = playlist.Id;
                playlist.PlaylistStatisticSnapshots.Add(playlistStatisticSnapshot);
                DbCtx.Add(playlistStatisticSnapshot);
            }
        }

        UpdateEntityImages(playlist, p => p.PlaylistImages, newPlaylistData.PlaylistImages, isNew, imageUpdateOptions);

        UpdateBaseEntity(playlist, newPlaylistData, isNew);

        // Not updating Author or PlaylistItems, they should be updated externally
        // TODO: StatusChangeEvents?
    }

    public enum EImageUpdateOptions
    {
        NoUpdate,
        ExpireNonMatching,
        OnlyAdd,
        OnlyAddSkipIfNotLoaded,
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
        UpdateEntityImages(author, a => a.AuthorImages, newAuthorData.AuthorImages, isNew, options.ImageUpdateOptions);

        // Skipping VideoAuthors, TODO???

        author.ArchivalSettings = newAuthorData.ArchivalSettings ?? author.ArchivalSettings;

        UpdateBaseEntity(author, newAuthorData, isNew);

        if (changed)
        {
            _dataUow.Ctx.AuthorHistories.Add(history);
        }
    }

    public void UpdateEntityImages<TEntity, TEntityImage, TCollection>(
        TEntity entity,
        Expression<Func<TEntity, TCollection?>> entityImagesAccessor,
        ICollection<TEntityImage>? newEntityImages,
        bool isNew,
        EImageUpdateOptions imageUpdateOptions
    )
        where TEntityImage : class, IEntityImage
        where TEntity : class, IIdDatabaseEntity
        where TCollection : ICollection<TEntityImage>
    {
        if (newEntityImages is not { Count: > 0 } || imageUpdateOptions == EImageUpdateOptions.NoUpdate)
        {
            return;
        }

        var (getter, setter, memberName) = entityImagesAccessor.GetGetterAndSetter();

        if (isNew)
        {
            if (getter(entity) == null)
            {
                TCollection newCollection;
                if (typeof(TCollection).IsAssignableFrom(typeof(List<TEntityImage>)))
                {
                    newCollection = (TCollection)(object)new List<TEntityImage>();
                }
                else if (typeof(TCollection).GetConstructor(Type.EmptyTypes) != null)
                {
                    newCollection = Activator.CreateInstance<TCollection>();
                }
                else
                {
                    throw new Exception(
                        $"Failed to construct default collection for member {memberName} for entity {entity.GetType().FullName} with ID {entity.Id}");
                }

                setter(entity, newCollection);
            }
        }

        var entityImages = getter(entity);

        if (entityImages == null)
        {
            HandleEntityImagesMissing(entity, imageUpdateOptions, memberName);
            return;
        }

        var currentTime = DateTimeOffset.UtcNow;
        var validEntityImages = new List<TEntityImage>();
        foreach (var newEntityImage in newEntityImages)
        {
            var existingEntityImage = entityImages.FirstOrDefault(ei =>
                ei.ValidUntil == null &&
                ei.Image.AssertNotNull($"Image {ei.ImageId} not loaded for {ei.GetType().FullName} with ID {ei.Id}")
                    .Url == newEntityImage.Image!.Url.AssertNotNull("URL missing for new Image"));
            if (existingEntityImage != null)
            {
                existingEntityImage.LastFetched = DateTimeOffset.UtcNow;
                validEntityImages.Add(existingEntityImage);
            }
            else
            {
                newEntityImage.ValidSince ??= currentTime;
                validEntityImages.Add(newEntityImage);
                entityImages.Add(newEntityImage);
                DbCtx.Add(newEntityImage);
            }
        }

        if (imageUpdateOptions == EImageUpdateOptions.ExpireNonMatching)
        {
            foreach (var authorImage in entityImages.Where(ai =>
                         ai.ValidUntil == null
                         && !validEntityImages.Contains(ai)))
            {
                authorImage.ValidUntil = currentTime;
            }
        }
    }

    private void HandleEntityImagesMissing<TEntity>(TEntity entity, EImageUpdateOptions imageUpdateOptions,
        string memberName) where TEntity : class, IIdDatabaseEntity
    {
        switch (imageUpdateOptions)
        {
            case EImageUpdateOptions.OnlyAddSkipIfNotLoaded:
                Logger.LogInformation(
                    "Skipping images ({ImagesProperty}) update for {EntityType} with ID {EntityId}, navigation not loaded",
                    memberName, entity.GetType().FullName, entity.Id
                );
                return;
            default:
                throw new Exception(
                    $"Can't update images ({memberName}) for {entity.GetType().FullName} with ID {entity.Id}, navigation not loaded");
        }
    }

    public void UpdateTranslations<TEntity>(TEntity entity,
        Expression<Func<TEntity, TextTranslationKey?>> translationsPropertyExpression,
        TextTranslationKey? newTranslationKey)
    {
        var (getter, setter, _) = translationsPropertyExpression.GetGetterAndSetter();
        var existingTranslationKey = getter(entity);
        if (existingTranslationKey == null)
        {
            existingTranslationKey = new TextTranslationKey();
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
            entity.AddedToArchiveAt = newEntityData.AddedToArchiveAt;
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