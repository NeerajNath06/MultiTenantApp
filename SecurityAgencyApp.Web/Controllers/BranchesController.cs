using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class BranchesController : Controller
{
    public IActionResult Index(string? search = null)
    {
        ViewBag.Search = search;
        return View(new BranchListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateBranchRequest { IsActive = true });
    }

    public IActionResult Edit(Guid id)
    {
        return View(new UpdateBranchRequest
        {
            Id = id,
            IsActive = true
        });
    }

    public IActionResult Details(Guid id)
    {
        ViewBag.BranchId = id;
        return View();
    }
}
