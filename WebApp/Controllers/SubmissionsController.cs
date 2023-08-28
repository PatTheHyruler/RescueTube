using BLL;
using BLL.Contracts.Exceptions;
using BLL.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels;

#pragma warning disable CS1591

namespace WebApp.Controllers;

[Authorize(Roles = RoleNames.AllowedToSubmitRoles)]
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
    public async Task<IActionResult> Create([FromForm] LinkSubmissionModel model)
    {
        try
        {
            var successResult = await _serviceUow.SubmissionService.SubmitGenericLinkAsync(model.Link, User);
            await _serviceUow.SaveChangesAsync();
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