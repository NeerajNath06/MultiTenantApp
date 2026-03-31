using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class LeaveRequestsController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null, Guid? guardId = null, string? status = null)
    {
        return View(new LeaveRequestListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateLeaveRequestRequest { StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1), LeaveType = "Casual" });
    }

    public IActionResult Details(Guid id)
    {
        ViewBag.LeaveRequestId = id;
        return View();
    }
}
