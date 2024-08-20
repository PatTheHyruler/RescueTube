using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Data;
using RescueTube.Core.Services;
using RescueTube.DAL.EF.Postgres;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Localization;
using RescueTube.Tests.Common.Logging;
using Xunit.Abstractions;

namespace RescueTube.Core.Tests;

public class EntityUpdateServiceTest
{
    private readonly IServiceCollection _serviceCollection;

    private IServiceScope CreateScope() => _serviceCollection.BuildServiceProvider().CreateScope();

    public EntityUpdateServiceTest(ITestOutputHelper output)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new List<KeyValuePair<string, string?>>
            {
                new("RescueTubePostgres", "fakeconnectionstring")
            })
            .Build();
        _serviceCollection = new ServiceCollection();
        _serviceCollection.AddSingleton<IConfiguration>(config);
        _serviceCollection.AddXunitLogging(output);
        _serviceCollection.AddDbPersistenceEfPostgres(config);
        _serviceCollection.AddBll();
    }

    [Fact]
    public void UpdateTranslations_AddNewTranslation_InvalidatesOriginal()
    {
        var firstTranslationContent = Guid.NewGuid().ToString();
        var originalTranslationKey = new TextTranslationKey
        {
            Translations = new List<TextTranslation>
            {
                new()
                {
                    Content = firstTranslationContent,
                    Culture = "en-GB",
                    ValidUntil = null,
                }
            },
        };
        var video = new Video
        {
            IdOnPlatform = string.Empty,
            Title = originalTranslationKey,
        };

        var secondTranslationContent = Guid.NewGuid().ToString();
        var newTranslationKey = new TextTranslationKey
        {
            Translations = new List<TextTranslation>
            {
                new()
                {
                    Content = secondTranslationContent,
                    Culture = "en-GB",
                    ValidUntil = null,
                }
            },
        };

        using var scope = CreateScope();
        var entityUpdateService = scope.ServiceProvider.GetRequiredService<EntityUpdateService>();
        entityUpdateService.UpdateTranslations(video, v => v.Title, newTranslationKey);

        Assert.Equal(originalTranslationKey.Id, video.Title.Id);
        Assert.NotEqual(newTranslationKey.Id, video.Title.Id);
        Assert.Same(originalTranslationKey, video.Title);

        Assert.Equal(2, video.Title.Translations.Count);
        Assert.Contains(video.Title.Translations, t => t.Content == firstTranslationContent);
        Assert.Contains(video.Title.Translations, t => t.Content == secondTranslationContent);

        var firstTranslationValidUntil =
            video.Title.Translations
                .Single(t => t.Content == firstTranslationContent)
                .ValidUntil;
        Assert.NotNull(firstTranslationValidUntil);
        Assert.True(firstTranslationValidUntil > DateTimeOffset.UtcNow.AddSeconds(-20)
                    && firstTranslationValidUntil < DateTimeOffset.UtcNow.AddSeconds(20));

        var secondTranslationValidUntil =
            video.Title.Translations
                .Single(t => t.Content == secondTranslationContent)
                .ValidUntil;
        Assert.Null(secondTranslationValidUntil);
    }

    [Fact]
    public void UpdateTranslations_AddNewTranslation_DoesNotInvalidateUnrelatedCulture()
    {
        var firstTranslationContent = Guid.NewGuid().ToString();
        var originalTranslationKey = new TextTranslationKey
        {
            Translations = new List<TextTranslation>
            {
                new()
                {
                    Content = firstTranslationContent,
                    Culture = "en-GB",
                    ValidUntil = null,
                }
            },
        };
        var video = new Video
        {
            IdOnPlatform = string.Empty,
            Title = originalTranslationKey,
        };

        var secondTranslationContent = Guid.NewGuid().ToString();
        var newTranslationKey = new TextTranslationKey
        {
            Translations = new List<TextTranslation>
            {
                new()
                {
                    Content = secondTranslationContent,
                    Culture = "en-US",
                    ValidUntil = null,
                }
            },
        };

        using var scope = CreateScope();
        var entityUpdateService = scope.ServiceProvider.GetRequiredService<EntityUpdateService>();
        entityUpdateService.UpdateTranslations(video, v => v.Title, newTranslationKey);

        Assert.Equal(originalTranslationKey.Id, video.Title.Id);
        Assert.NotEqual(newTranslationKey.Id, video.Title.Id);
        Assert.Same(originalTranslationKey, video.Title);

        Assert.Equal(2, video.Title.Translations.Count);
        Assert.Contains(video.Title.Translations, t => t.Content == firstTranslationContent);
        Assert.Contains(video.Title.Translations, t => t.Content == secondTranslationContent);

        var firstTranslationValidUntil =
            video.Title.Translations
                .Single(t => t.Content == firstTranslationContent)
                .ValidUntil;
        Assert.Null(firstTranslationValidUntil);

        var secondTranslationValidUntil =
            video.Title.Translations
                .Single(t => t.Content == secondTranslationContent)
                .ValidUntil;
        Assert.Null(secondTranslationValidUntil);
    }

    [Fact]
    public void UpdateTranslations_AddNewTranslation_DoesNotAddIfMatchesPrevious()
    {
        var translationContent = Guid.NewGuid().ToString();
        var originalTranslationKey = new TextTranslationKey
        {
            Translations = new List<TextTranslation>
            {
                new()
                {
                    Content = translationContent,
                    Culture = "en-GB",
                    ValidUntil = null,
                }
            },
        };
        var video = new Video
        {
            IdOnPlatform = string.Empty,
            Title = originalTranslationKey,
        };

        var newTranslationKey = new TextTranslationKey
        {
            Translations = new List<TextTranslation>
            {
                new()
                {
                    Content = translationContent,
                    Culture = "en-GB",
                    ValidUntil = null,
                }
            },
        };

        using var scope = CreateScope();
        var entityUpdateService = scope.ServiceProvider.GetRequiredService<EntityUpdateService>();
        entityUpdateService.UpdateTranslations(video, v => v.Title, newTranslationKey);

        Assert.Equal(originalTranslationKey.Id, video.Title.Id);
        Assert.NotEqual(newTranslationKey.Id, video.Title.Id);
        Assert.Same(originalTranslationKey, video.Title);

        Assert.Equal(1, video.Title.Translations.Count);
        Assert.Contains(video.Title.Translations, t => t.Content == translationContent);

        var translationValidUntil =
            video.Title.Translations
                .Single()
                .ValidUntil;
        Assert.Null(translationValidUntil);
    }

    [Fact]
    public void UpdateTranslations_AddNewTranslation_CreatesNewKeyIfNecessary()
    {
        var video = new Video
        {
            IdOnPlatform = string.Empty,
        };

        var translationContent = Guid.NewGuid().ToString();
        var newTranslationKey = new TextTranslationKey
        {
            Translations = new List<TextTranslation>
            {
                new()
                {
                    Content = translationContent,
                    Culture = "en-GB",
                    ValidUntil = null,
                }
            },
        };

        using var scope = CreateScope();
        var entityUpdateService = scope.ServiceProvider.GetRequiredService<EntityUpdateService>();
        entityUpdateService.UpdateTranslations(video, v => v.Title, newTranslationKey);

        Assert.NotNull(video.Title);
        Assert.NotNull(video.Title.Translations);
        Assert.Equal(1, video.Title.Translations.Count);
        Assert.Equal(translationContent, video.Title.Translations.Single().Content);
    }

    [Fact]
    public void UpdateEntityImages_ExpireNonMatching_LeavesEntityImagesInCorrectState()
    {
        // Arrange
        VideoImage CreateFakeVideoImage(string? url = null)
        {
            return new VideoImage
            {
                Image = new Image
                {
                    Url = url ?? Guid.NewGuid().ToString(),
                }
            };
        }

        var video = new Video
        {
            IdOnPlatform = string.Empty,
        };

        var existingVideoImageFound = CreateFakeVideoImage();
        var existingVideoImageRemoved = CreateFakeVideoImage();

        var addedVideoImageUrl = Guid.NewGuid().ToString();

        video.VideoImages = [existingVideoImageFound, existingVideoImageRemoved];

        using var scope = CreateScope();
        var dbCtx = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        dbCtx.Entry(video).State = EntityState.Unchanged;
        dbCtx.Entry(existingVideoImageFound).State = EntityState.Unchanged;
        dbCtx.Entry(existingVideoImageFound.Image!).State = EntityState.Unchanged;
        dbCtx.Entry(existingVideoImageRemoved).State = EntityState.Unchanged;
        dbCtx.Entry(existingVideoImageRemoved.Image!).State = EntityState.Unchanged;

        var startTime = DateTimeOffset.UtcNow;

        var entityUpdateService = scope.ServiceProvider.GetRequiredService<EntityUpdateService>();

        // Act
        entityUpdateService.UpdateEntityImages(video, v => v.VideoImages, [
                CreateFakeVideoImage(existingVideoImageFound.Image!.Url),
                CreateFakeVideoImage(addedVideoImageUrl),
            ], false,
            EntityUpdateService.EImageUpdateOptions.ExpireNonMatching);

        // Assert
        var entries = dbCtx.ChangeTracker.Entries<VideoImage>().ToList();
        Assert.Equal(3, entries.Count);

        var addedEntry = entries.Find(e => e.Entity.Image!.Url == addedVideoImageUrl);
        Assert.NotNull(addedEntry);
        Assert.Equal(EntityState.Added, addedEntry.State);
        Assert.NotNull(addedEntry.Entity.Image);
        var addedImageEntry = dbCtx.Entry(addedEntry.Entity.Image);
        Assert.NotNull(addedImageEntry);
        Assert.Equal(EntityState.Added, addedImageEntry.State);

        var existingEntry = entries.Find(e => e.Entity.Image!.Url == existingVideoImageFound.Image!.Url);
        Assert.NotNull(existingEntry);
        Assert.Equal(EntityState.Modified, existingEntry.State);
        Assert.NotNull(existingEntry.Entity.Image);
        var existingImageEntry = dbCtx.Entry(existingEntry.Entity.Image);
        Assert.NotNull(existingImageEntry);
        Assert.Equal(EntityState.Unchanged, existingImageEntry.State);

        var removedEntry = entries.Find(e => e.Entity.Image!.Url == existingVideoImageRemoved.Image!.Url);
        Assert.NotNull(removedEntry);
        Assert.Equal(EntityState.Modified, removedEntry.State);
        Assert.NotNull(removedEntry.Entity.Image);
        var removedImageEntry = dbCtx.Entry(removedEntry.Entity.Image);
        Assert.NotNull(removedImageEntry);
        Assert.Equal(EntityState.Unchanged, removedImageEntry.State);

        Assert.Null(existingEntry.Entity.ValidUntil);
        Assert.Null(addedEntry.Entity.ValidUntil);
        Assert.NotNull(removedEntry.Entity.ValidUntil);
        Assert.True(IsBetweenStartTimeAndCurrent(removedEntry.Entity.ValidUntil.Value, startTime));

        Assert.Equal(3, video.VideoImages.Count);
        Assert.Contains(existingEntry.Entity, video.VideoImages);
        Assert.Contains(addedEntry.Entity, video.VideoImages);
        Assert.Contains(removedEntry.Entity, video.VideoImages);
    }

    private static bool IsBetweenStartTimeAndCurrent(DateTimeOffset value, DateTimeOffset startTime)
    {
        return value >= startTime && value <= DateTimeOffset.UtcNow;
    }
}