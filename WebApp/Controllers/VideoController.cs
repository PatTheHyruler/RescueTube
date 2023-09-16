using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels;

namespace WebApp.Controllers;

public class VideoController : Controller
{
    private readonly ServiceUow _serviceUow;

    public VideoController(ServiceUow serviceUow)
    {
        _serviceUow = serviceUow;
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

    public IActionResult Watch(Guid id)
    {
        return Ok(id);
    }
}