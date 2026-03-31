using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class SitePostsController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null, Guid? siteId = null, Guid? branchId = null)
    {
        ViewBag.Search = search;
        return View(new SitePostListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateSitePostRequest());
    }
    public IActionResult Edit(Guid id)
    {
        return View(new UpdateSitePostRequest
        {
            Id = id,
            IsActive = true
        });
    }

    public IActionResult Details(Guid id)
    {
        ViewBag.SitePostId = id;
        return View();
    }
}
