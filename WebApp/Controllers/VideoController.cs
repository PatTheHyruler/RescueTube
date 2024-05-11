using RescueTube.Core;
using HeyRed.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApp.ViewModels.Video;

namespace WebApp.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class VideoController : Controller
{
    private readonly ServiceUow _serviceUow;
    private readonly IWebHostEnvironment _environment;
    private readonly IOptions<AppPathOptions> _appPathOptions;

    public VideoController(ServiceUow serviceUow, IWebHostEnvironment environment, IOptions<AppPathOptions> appPathOptions)
    {
        _serviceUow = serviceUow;
        _environment = environment;
        _appPathOptions = appPathOptions;
    }

    [Authorize]
    public async Task<IActionResult> Search(VideoSearchQueryModel model)
    {
        var viewModel = new VideoSearchViewModel
        {
            NameQuery = model.NameQuery,
            AuthorQuery = model.AuthorQuery,
            Page = model.Page,
            Limit = model.Limit,
            SortingOptions = model.SortingOptions,
            Descending = model.Descending,
            Videos = await _serviceUow.VideoPresentationService.SearchVideosAsync(
                platformQuery: null, /*TODO*/ nameQuery: model.NameQuery, authorQuery: model.AuthorQuery,
                categoryIds: null, // TODO
                user: User, userAuthorId: null, // TODO
                page: model.Page, limit: model.Limit,
                sortingOptions: model.SortingOptions, descending: model.Descending
            )
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Watch([FromRoute] Guid id, [FromQuery] VideoWatchViewModel model)
    {
        if (!await _serviceUow.AuthorizationService.IsAllowedToAccessVideo(User, id))
        {
            return NotFound();
        }

        var video = await _serviceUow.VideoPresentationService.GetVideoSimple(id);
        if (video == null)
        {
            return NotFound();
        }
        model.Video = video;
        
        return View(model);
    }

    [HttpGet("/[controller]/[action]/{videoId:guid}")]
    [AllowAnonymous]
    [Authorize]
    public async Task<IResult> VideoFile([FromRoute] Guid videoId)
    {
        if (!await _serviceUow.AuthorizationService.IsAllowedToAccessVideo(User, videoId))
        {
            return Results.NotFound();
        }

        var videoFile = await _serviceUow.VideoPresentationService.GetVideoFileAsync(videoId);
        if (videoFile == null)
        {
            return Results.NotFound();
        }

        var filePath = videoFile.FilePath;

        var contentType = MimeTypesMap.GetMimeType(filePath);

        FileStream stream;
        try
        {
            // Using ContentRootPath is necessary on some OSes / hosting scenarios?
            stream = System.IO.File.OpenRead(Path.Combine(
                _environment.ContentRootPath, _appPathOptions.Value.Downloads, filePath));
        }
        catch (FileNotFoundException)
        {
            return Results.NotFound();
        }
        catch (DirectoryNotFoundException)
        {
            return Results.NotFound();
        }

        return Results.File(stream, contentType, enableRangeProcessing: true);
    }
}