using RescueTube.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels.Comment;

namespace WebApp.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class CommentController : Controller
{
    private readonly CommentService _commentService;
    private readonly AuthorizationService _authorizationService;

    public CommentController(CommentService commentService, AuthorizationService authorizationService)
    {
        _commentService = commentService;
        _authorizationService = authorizationService;
    }

    [HttpGet("[Controller]/[Action]/{videoId:guid}")]
    [Authorize]
    [AllowAnonymous]
    public async Task<IActionResult> VideoComments([FromRoute] Guid videoId, [FromQuery] VideoCommentsQueryViewModel model)
    {
        if (!await _authorizationService.IsVideoAccessAllowed(videoId, User))
        {
            return NotFound();
        }
        var response = await _commentService.GetVideoComments(videoId, model);
        if (response == null)
        {
            return NotFound();
        }

        var viewModel = new VideoCommentsViewModel
        {
            Limit = response.PaginationResult.Limit,
            Page = response.PaginationResult.Page,
            PaginationResult = response.PaginationResult,
            VideoComments = response.Result,
        };
        
        return View(viewName: "_VideoCommentsPartial", viewModel);
    }
}