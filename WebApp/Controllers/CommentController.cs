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
    public async Task<IActionResult> VideoComments([FromRoute] Guid videoId, [FromQuery] VideoCommentsViewModel model)
    {
        if (!await _authorizationService.IsAllowedToAccessVideo(User, videoId))
        {
            return NotFound();
        }
        var videoComments = await _commentService.GetVideoComments(videoId, model);
        if (videoComments == null)
        {
            return NotFound();
        }

        model.VideoComments = videoComments;
        
        return View(viewName: "_VideoCommentsPartial", model);
    }
}