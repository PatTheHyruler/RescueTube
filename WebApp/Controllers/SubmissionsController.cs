using RescueTube.Core;
using RescueTube.Core.Exceptions;
using RescueTube.Core.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels;

namespace WebApp.Controllers;

[Authorize(Roles = RoleNames.AllowedToSubmitRoles)]
[ApiExplorerSettings(IgnoreApi = true)]
public class SubmissionsController : Controller
{
    private readonly ServiceUow _serviceUow;

    public SubmissionsController(ServiceUow serviceUow)
    {
        _serviceUow = serviceUow;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] LinkSubmissionModel model, CancellationToken ct = default)
    {
        try
        {
            var successResult = await _serviceUow.SubmissionService.SubmitGenericLinkAsync(model.Link, User, ct);
            await _serviceUow.SaveChangesAsync(ct);
            return RedirectToAction(nameof(Details), new { Id = successResult.SubmissionId });
        }
        catch (UnrecognizedUrlException e)
        {
            ModelState.AddModelError(nameof(model.Link), e.Message);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Sorry, something went wrong");
        }

        return View(model);
    }

    [HttpGet("[controller]/{id:guid}")]
    public IActionResult Details(Guid id)
    {
        return View(viewName: "Details", model: id);
    }
}