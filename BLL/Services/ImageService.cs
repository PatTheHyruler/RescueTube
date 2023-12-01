using System.Security.Cryptography;
using System.Text;
using BLL.Base;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BLL.Services;

public class ImageService : BaseService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ImageService(IServiceProvider services, ILogger<ImageService> logger, IHttpClientFactory httpClientFactory) :
        base(services, logger)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task UpdateImage(Guid imageId, CancellationToken ct)
    {
        var image = await Ctx.Images
            .Where(e => e.Id == imageId)
            .Include(e => e.AuthorImages)
            .Include(e => e.VideoImages)
            .FirstAsync(ct);
        await UpdateImage(image, ct);
    }

    public async Task UpdateImage(Image image, CancellationToken ct)
    {
        if (image.Url == null) return;
        using var httpClient = _httpClientFactory.CreateClient(); // TODO: should this be a class member instead?
        var response = await httpClient.GetAsync(image.Url, ct);
        if (!response.IsSuccessStatusCode)
        {
            // Maybe this should be done immediately in the DB to avoid concurrency issues
            // But the worst outcome of concurrency issues here would be a few extra download attempts
            image.FailedFetchAttempts++;
            return;
        }

        bool? isChanged = null;

        if (isChanged == null &&
            image.Etag != null && response.Headers.ETag != null)
        {
            isChanged = image.Etag != response.Headers.ETag.Tag;
        }

        var imageBytes = await response.Content.ReadAsByteArrayAsync(ct);
        ct.ThrowIfCancellationRequested();

        var hash = MD5.HashData(imageBytes);
        var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();

        if (isChanged == null
            && image.Hash != null)
        {
            isChanged = hashString == image.Hash;
        }

        Image imageToUpdate;
        if (isChanged == true && image.LocalFilePath != null)
        {
            imageToUpdate = CreateUpdatedImage(image);
        }
        else
        {
            imageToUpdate = image;
        }

        imageToUpdate.Hash = hashString;
        imageToUpdate.MediaType = response.Content.Headers.ContentType?.MediaType;
        imageToUpdate.Etag = response.Headers.ETag?.Tag;
        imageToUpdate.Ext ??= GetFileExtension(response);
        // TODO: Try to parse ext from URL if still null?

        if (imageToUpdate.LocalFilePath == null)
        {
            var appPathOptions = Services.GetService<IOptions<AppPathOptions>>()?.Value;
            await DownloadImage(imageToUpdate, appPathOptions, imageBytes, ct);
        }
    }

    private static async Task DownloadImage(Image image,
        AppPathOptions? appPathOptions, byte[] imageBytes, CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return;
        var fileNameBuilder = new StringBuilder()
            .Append(image.Url?.ToFileNameSanitized(100) ?? "NULL")
            .Append('_')
            .Append(DateTime.UtcNow.Ticks)
            .Append('_')
            .Append(Guid.NewGuid().ToString().Replace("-", ""));
        if (image.Ext != null)
        {
            fileNameBuilder.Append(".")
                .Append(image.Ext);
        }

        var imageDirectory = AppPaths.GetImagesDirectory(image.Platform, appPathOptions);
        Directory.CreateDirectory(imageDirectory);
        var imagePath = Path.Combine(
            imageDirectory,
            fileNameBuilder.ToString()
        );

        await using var fileStream = new FileStream(imagePath, FileMode.Create, FileAccess.Write);
        await fileStream.WriteAsync(imageBytes, ct);
        if (ct.IsCancellationRequested) return;
        image.LocalFilePath = AppPaths.GetPathRelativeToDownloads(imagePath, appPathOptions);
    }

    private Image CreateUpdatedImage(Image image)
    {
        var newImage = new Image
        {
            Platform = image.Platform,
            IdOnPlatform = image.IdOnPlatform,

            Key = image.Key,
            Quality = image.Quality,
            Ext = image.Ext,
            MediaType = image.MediaType,
            Url = image.Url,
            FailedFetchAttempts = image.FailedFetchAttempts,
            Width = image.Width,
            Height = image.Height,
        };
        Ctx.Images.Add(newImage);

        var currentTime = DateTime.UtcNow;

        if (image.VideoImages != null)
        {
            foreach (var videoImage in image.VideoImages)
            {
                videoImage.ValidUntil = currentTime;
                Ctx.VideoImages.Add(new VideoImage
                {
                    ImageType = videoImage.ImageType,
                    ValidSince = currentTime,
                    LastFetched = videoImage.LastFetched,
                    Preference = videoImage.Preference,
                    VideoId = videoImage.VideoId,
                    Image = newImage,
                });
            }
        }

        if (image.AuthorImages != null)
        {
            foreach (var authorImage in image.AuthorImages)
            {
                authorImage.ValidUntil = currentTime;
                Ctx.AuthorImages.Add(new AuthorImage
                {
                    ImageType = authorImage.ImageType,
                    ValidSince = currentTime,
                    LastFetched = authorImage.LastFetched,
                    AuthorId = authorImage.AuthorId,
                    Image = newImage,
                });
            }
        }

        return newImage;
    }

    private static string? GetFileExtension(HttpResponseMessage response)
    {
        string? ext = null;
        var contentDispositionFileName = response.Content.Headers.ContentDisposition?.FileName;
        if (contentDispositionFileName != null)
        {
            var splitFileName = contentDispositionFileName.Split('.');
            if (splitFileName.Length > 1)
            {
                var potentialExt = splitFileName[1];
                if (potentialExt.Trim().Length > 0)
                {
                    ext = potentialExt.ToFileNameSanitized();
                }
            }
        }

        if (ext != null) return ext;

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        ext = AppPaths.GuessImageFileExtensionFromMediaType(mediaType);

        return ext;
    }
}