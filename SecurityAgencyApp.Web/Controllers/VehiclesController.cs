using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class VehiclesController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null, Guid? siteId = null, Guid? guardId = null, DateTime? dateFrom = null, DateTime? dateTo = null, bool? insideOnly = null, string? sortBy = null, string? sortDirection = "desc")
    {
        return View(new VehicleLogListResponse());
    }

    public IActionResult Summary(Guid? siteId = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        return View(new VehicleLogSummaryBySiteResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateVehicleEntryRequest());
    }

    public IActionResult Details(Guid id)
    {
        ViewBag.VehicleLogId = id;
        return View();
    }
}
