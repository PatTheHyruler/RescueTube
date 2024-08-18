using RescueTube.Core.Utils;
using RescueTube.Domain.Entities;
using RescueTube.Domain.Enums;

namespace RescueTube.YouTube.Utils;

public static class ImageUtils
{
    private static EImageType? TryGetAuthorImageType(AuthorImage authorImage)
    {
        var image = authorImage.Image.AssertNotNull($"Image was not loaded for AuthorImage {authorImage.Id}");

        authorImage.ImageTypeDetectionAttempts += 1;

        if (image.IdOnPlatform?.Contains("avatar", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            return EImageType.ProfilePicture;
        }

        if (image.IdOnPlatform?.Contains("banner", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            return EImageType.Banner;
        }

        if (image is { Width: > 0, Height: > 0 })
        {
            var diff = Math.Abs(image.Width.Value - image.Height.Value);
            if (diff < 0.01 * image.Width && diff < 0.01 * image.Height)
            {
                return EImageType.ProfilePicture;
            }
        }

        return null;
    }

    public static AuthorImage TrySetImageType(this AuthorImage authorImage)
    {
        authorImage.ImageType ??= TryGetAuthorImageType(authorImage);
        return authorImage;
    }
}