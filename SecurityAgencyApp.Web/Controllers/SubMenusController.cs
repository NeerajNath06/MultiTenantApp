using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class SubMenusController : Controller
{
    public IActionResult Index(Guid? menuId = null)
    {
        ViewBag.SelectedMenuId = menuId;
        return View(new SubMenuListResponse());
    }

    public IActionResult Create(Guid menuId)
    {
        return View(new CreateSubMenuRequest { MenuId = menuId, IsActive = true });
    }

    public IActionResult Edit(Guid id)
    {
        return View(new UpdateSubMenuRequest { Id = id, IsActive = true });
    }
}
