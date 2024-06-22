using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RescueTube.Core.Services;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Entities.Localization;

namespace RescueTube.Core.Tests;

public class EntityUpdateServiceTest
{
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

        EntityUpdateService.UpdateTranslations(video, v => v.Title, newTranslationKey);

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

        EntityUpdateService.UpdateTranslations(video, v => v.Title, newTranslationKey);

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

        EntityUpdateService.UpdateTranslations(video, v => v.Title, newTranslationKey);

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
        
        EntityUpdateService.UpdateTranslations(video, v => v.Title, newTranslationKey);
        
        Assert.NotNull(video.Title);
        Assert.NotNull(video.Title.Translations);
        Assert.Equal(1, video.Title.Translations.Count);
        Assert.Equal(translationContent, video.Title.Translations.Single().Content);
    }
}