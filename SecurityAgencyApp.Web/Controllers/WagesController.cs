using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class WagesController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? status = null)
    {
        return View(new WageListResponse());
    }

    public IActionResult Create()
    {
        var endDate = DateTime.UtcNow;
        var startDate = new DateTime(endDate.Year, endDate.Month, 1);
        var periodEnd = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));
        return View(new CreateWageRequest
        {
            WagePeriodStart = startDate,
            WagePeriodEnd = periodEnd,
            PaymentDate = endDate.AddDays(7),
            Status = "Draft"
        });
    }

    public IActionResult Details(Guid id)
    {
        ViewBag.WageId = id;
        return View();
    }
}
