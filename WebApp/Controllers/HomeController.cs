using BLL.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable CS1591
namespace WebApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [Authorize(Roles = RoleNames.Admin)]
    public IActionResult Secret()
    {
        return View();
    }
}