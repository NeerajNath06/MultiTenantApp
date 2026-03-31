using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class UsersController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null)
    {
        return View(new UserListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateUserRequest());
    }

    public IActionResult Edit(Guid id)
    {
        return View(new UpdateUserRequest { Id = id, RoleIds = new List<Guid>() });
    }

    public IActionResult Details(Guid id)
    {
        ViewBag.UserId = id;
        return View();
    }
}
