using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using RescueTube.Core;
using RescueTube.Core.Data;
using RescueTube.Core.Services;
using RescueTube.DAL.EF.Postgres;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Localization;

namespace RescueTube.Tests.Core.Services;

public class EntityUpdateServiceTests
{
    private readonly IServiceCollection _serviceCollection;

    private IServiceScope CreateScope() => _serviceCollection.BuildServiceProvider().CreateScope();

    private readonly FakeTimeProvider _timeProvider = new(new(2025, 07, 10, 12, 34, 56, TimeSpan.Zero))
    {
        AutoAdvanceAmount = TimeSpan.FromSeconds(1),
    };

    public EntityUpdateServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new List<KeyValuePair<string, string?>>
            {
                new("RescueTubePostgres", "fakeconnectionstring")
            })
            .Build();
        _serviceCollection = new ServiceCollection();
        _serviceCollection.AddSingleton<TimeProvider>(_timeProvider);
        _serviceCollection.AddSingleton<IConfiguration>(config);
        _serviceCollection.AddLogging(b => b.AddConsole());
        _serviceCollection.AddDbPersistenceEfPostgres(config);
        _serviceCollection.AddBll();
    }

    [Test]
    public async Task UpdateTranslations_AddNewTranslation_InvalidatesOriginal()
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

        await Assert.That(originalTranslationKey.Id).IsEqualTo(video.Title.Id);
        await Assert.That(newTranslationKey.Id).IsNotEqualTo(video.Title.Id);
        await Assert.That(originalTranslationKey).IsSameReferenceAs(video.Title);

        await Assert.That(video.Title.Translations)
            .IsNotNull()
            .And.Count().IsEqualTo(2)
            .And.Contains(t => t.Content == firstTranslationContent)
            .And.Contains(t => t.Content == secondTranslationContent);

        var firstTranslationValidUntil =
            video.Title.Translations
                .Single(t => t.Content == firstTranslationContent)
                .ValidUntil;
        await Assert.That(firstTranslationValidUntil).IsNotNull();
        await Assert.That(firstTranslationValidUntil!.Value) // TODO: better dev ergonomics for this?
            .IsGreaterThan(DateTimeOffset.UtcNow.AddSeconds(-20)) // TODO: use TimeProvider in solution?
            .And.IsLessThan(DateTimeOffset.UtcNow.AddSeconds(20));

        var secondTranslationValidUntil =
            video.Title.Translations
                .Single(t => t.Content == secondTranslationContent)
                .ValidUntil;
        await Assert.That(secondTranslationValidUntil).IsNull();
    }

    [Test]
    public async Task UpdateTranslations_AddNewTranslation_DoesNotInvalidateUnrelatedCulture()
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

        await Assert.That(originalTranslationKey.Id).IsEqualTo(video.Title.Id);
        await Assert.That(newTranslationKey.Id).IsNotEqualTo(video.Title.Id);
        await Assert.That(originalTranslationKey).IsSameReferenceAs(video.Title);

        await Assert.That(video.Title.Translations)
            .IsNotNull()
            .And.Count().IsEqualTo(2)
            .And.Contains(t => t.Content == firstTranslationContent)
            .And.Contains(t => t.Content == secondTranslationContent);

        var firstTranslationValidUntil =
            video.Title.Translations
                .Single(t => t.Content == firstTranslationContent)
                .ValidUntil;
        await Assert.That(firstTranslationValidUntil).IsNull();

        var secondTranslationValidUntil =
            video.Title.Translations
                .Single(t => t.Content == secondTranslationContent)
                .ValidUntil;
        await Assert.That(secondTranslationValidUntil).IsNull();
    }

    [Test]
    public async Task UpdateTranslations_AddNewTranslation_DoesNotAddIfMatchesPrevious()
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

        await Assert.That(originalTranslationKey.Id).IsEqualTo(video.Title.Id);
        await Assert.That(newTranslationKey.Id).IsNotEqualTo(video.Title.Id);
        await Assert.That(originalTranslationKey).IsSameReferenceAs(video.Title);

        await Assert.That(video.Title.Translations)
            .Count().IsEqualTo(1)
            .And.ContainsOnly(t => t.Content == translationContent);

        var translationValidUntil =
            video.Title.Translations
                .Single()
                .ValidUntil;
        await Assert.That(translationValidUntil).IsNull();
    }

    [Test]
    public async Task UpdateTranslations_AddNewTranslation_CreatesNewKeyIfNecessary()
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

        await Assert.That(video.Title?.Translations)
            .IsNotNull()
            .And.Count().IsEqualTo(1)
            .And.ContainsOnly(t => t.Content == translationContent);
    }

    [Test]
    public async Task UpdateEntityImages_ExpireNonMatching_LeavesEntityImagesInCorrectState()
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
        await Assert.That(entries.Count).IsEqualTo(3);

        var addedEntry = entries.Find(e => e.Entity.Image!.Url == addedVideoImageUrl);
        await Assert.That(addedEntry).IsNotNull();
        await Assert.That(addedEntry!.State).IsEqualTo(EntityState.Added);
        await Assert.That(addedEntry.Entity.Image).IsNotNull();
        var addedImageEntry = dbCtx.Entry(addedEntry.Entity.Image!);
        await Assert.That(addedImageEntry).IsNotNull();
        await Assert.That(addedImageEntry.State).IsEqualTo(EntityState.Added);

        var existingEntry = entries.Find(e => e.Entity.Image!.Url == existingVideoImageFound.Image!.Url);
        await Assert.That(existingEntry).IsNotNull();
        await Assert.That(existingEntry!.State).IsEqualTo(EntityState.Modified);
        await Assert.That(existingEntry.Entity.Image).IsNotNull();
        var existingImageEntry = dbCtx.Entry(existingEntry.Entity.Image!);
        await Assert.That(existingImageEntry).IsNotNull();
        await Assert.That(existingImageEntry.State).IsEqualTo(EntityState.Unchanged);

        var removedEntry = entries.Find(e => e.Entity.Image!.Url == existingVideoImageRemoved.Image!.Url);
        await Assert.That(removedEntry).IsNotNull();
        await Assert.That(removedEntry!.State).IsEqualTo(EntityState.Modified);
        await Assert.That(removedEntry.Entity.Image).IsNotNull();
        var removedImageEntry = dbCtx.Entry(removedEntry.Entity.Image!);
        await Assert.That(removedImageEntry).IsNotNull();
        await Assert.That(removedImageEntry.State).IsEqualTo(EntityState.Unchanged);

        await Assert.That(existingEntry.Entity.ValidUntil).IsNull();
        await Assert.That(addedEntry.Entity.ValidUntil).IsNull();
        await Assert.That(removedEntry.Entity.ValidUntil).IsNotNull();
        await Assert.That(IsBetweenStartTimeAndCurrent(removedEntry.Entity.ValidUntil!.Value, startTime)).IsTrue();

        await Assert.That(video.VideoImages.Count).IsEqualTo(3);
        await Assert.That(video.VideoImages)
            .Contains(existingEntry.Entity)
            .And.Contains(addedEntry.Entity)
            .And.Contains(removedEntry.Entity);
    }

    private static bool IsBetweenStartTimeAndCurrent(DateTimeOffset value, DateTimeOffset startTime)
    {
        return value >= startTime && value <= DateTimeOffset.UtcNow;
    }
}