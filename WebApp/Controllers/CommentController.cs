using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels.Comment;

namespace WebApp.Controllers;

public class CommentController : Controller
{
    private readonly CommentService _commentService;

    public CommentController(CommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("[Controller]/[Action]/{videoId:guid}")]
    public async Task<IActionResult> VideoComments([FromRoute] Guid videoId, [FromQuery] VideoCommentsViewModel model)
    {
        var videoComments = await _commentService.GetVideoComments(videoId, model);
        if (videoComments == null)
        {
            return NotFound();
        }

        model.VideoComments = videoComments;
        
        return View(viewName: "_VideoCommentsPartial", model);
    }
}