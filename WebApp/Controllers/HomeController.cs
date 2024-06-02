using RescueTube.Core.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [Authorize(Roles = RoleNames.AdminOrSuperAdmin)]
    public IActionResult Secret()
    {
        return Ok(new { Message = "Nice, you can access the secret :D" });
    }
}