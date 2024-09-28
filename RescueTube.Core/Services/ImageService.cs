using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Base;
using RescueTube.Core.Utils;
using RescueTube.Domain.Entities;

namespace RescueTube.Core.Services;

public class ImageService : BaseService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AppPaths _appPaths;

    public ImageService(IServiceProvider services, ILogger<ImageService> logger, IHttpClientFactory httpClientFactory,
        AppPaths appPaths) : base(services, logger)
    {
        _httpClientFactory = httpClientFactory;
        _appPaths = appPaths;
    }

    public async Task TryUpdateResolutionFromFileAsync(Guid imageId, CancellationToken ct)
    {
        var image = await DbCtx.Images
            .Where(e => e.Id == imageId)
            .FirstOrDefaultAsync(cancellationToken: ct);
        if (image == null) return;
        await TryUpdateResolutionFromFileAsync(image, ct);
    }

    public async Task TryUpdateResolutionFromFileAsync(Image image, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(image.LocalFilePath)) return;
        image.ResolutionParseAttemptedAt = DateTimeOffset.UtcNow;
        try
        {
            var absolutePath = _appPaths.GetAbsolutePathFromDownloads(image.LocalFilePath);
            var imageInfo = await SixLabors.ImageSharp.Image.IdentifyAsync(File.OpenRead(absolutePath), ct);
            image.Width ??= imageInfo.Width;
            image.Height ??= imageInfo.Height;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to update resolution from file for image {ImageId}", image.Id);
        }
    }

    public async Task UpdateImage(Guid imageId, CancellationToken ct)
    {
        var image = await DbCtx.Images
            .Where(e => e.Id == imageId)
            .Include(e => e.AuthorImages)
            .Include(e => e.VideoImages)
            .Include(e => e.PlaylistImages)
            .AsSingleQuery()
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
            isChanged = hashString != image.Hash;
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
            await DownloadImage(imageToUpdate, imageBytes, ct);
        }
    }

    private async Task DownloadImage(Image image, byte[] imageBytes, CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return;
        var fileNameBuilder = new StringBuilder()
            .Append(image.Url?.ToFileNameSanitized(100) ?? "NULL")
            .Append('_')
            .Append(DateTimeOffset.UtcNow.Ticks)
            .Append('_')
            .Append(Guid.NewGuid().ToString().Replace("-", ""));
        if (image.Ext != null)
        {
            fileNameBuilder.Append('.')
                .Append(image.Ext);
        }

        var imageDirectory = _appPaths.GetImagesDirectory(image.Platform);
        Directory.CreateDirectory(imageDirectory);
        var imagePath = Path.Combine(
            imageDirectory,
            fileNameBuilder.ToString()
        );

        await using var fileStream = new FileStream(imagePath, FileMode.Create, FileAccess.Write);
        await fileStream.WriteAsync(imageBytes, ct);
        if (ct.IsCancellationRequested) return;
        image.LocalFilePath = _appPaths.GetPathRelativeToDownloads(imagePath);
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
        DbCtx.Images.Add(newImage);

        var currentTime = DateTimeOffset.UtcNow;

        if (image.VideoImages != null)
        {
            foreach (var videoImage in image.VideoImages)
            {
                videoImage.ValidUntil = currentTime;
                DbCtx.VideoImages.Add(new VideoImage
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
                DbCtx.AuthorImages.Add(new AuthorImage
                {
                    ImageType = authorImage.ImageType,
                    ValidSince = currentTime,
                    LastFetched = authorImage.LastFetched,
                    AuthorId = authorImage.AuthorId,
                    Image = newImage,
                });
            }
        }

        if (image.PlaylistImages != null)
        {
            foreach (var playlistImage in image.PlaylistImages)
            {
                playlistImage.ValidUntil = currentTime;
                DbCtx.PlaylistImages.Add(new PlaylistImage
                {
                    ImageType = playlistImage.ImageType,
                    ValidSince = currentTime,
                    LastFetched = playlistImage.LastFetched,
                    PlaylistId = playlistImage.PlaylistId,
                    Image = newImage,
                });
            }
        }

        return newImage;
    }

    private static string? GetFileExtension(HttpResponseMessage response)
    {
        var contentDispositionFileName = response.Content.Headers.ContentDisposition?.FileName;
        if (contentDispositionFileName != null)
        {
            var splitFileName = contentDispositionFileName.Split('.');
            if (splitFileName.Length > 1)
            {
                var potentialExt = splitFileName[^1]
                    .Trim()
                    .Trim('"')
                    .Trim()
                    .ToFileNameSanitized();

                if (potentialExt.Length > 0)
                {
                    return potentialExt;
                }
            }
        }

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        return PathUtils.GuessImageFileExtensionFromMediaType(mediaType);
    }
}