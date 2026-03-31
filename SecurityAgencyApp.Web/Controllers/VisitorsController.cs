using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class VisitorsController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null, Guid? siteId = null, Guid? guardId = null, DateTime? dateFrom = null, DateTime? dateTo = null, bool? insideOnly = null, string? sortBy = null, string? sortDirection = "desc")
    {
        return View(new VisitorListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateVisitorRequest());
    }

    public IActionResult Details(Guid id)
    {
        ViewBag.VisitorId = id;
        return View();
    }

    public IActionResult Edit(Guid id)
    {
        return View(new UpdateVisitorRequest
        {
            Id = id
        });
    }
}
