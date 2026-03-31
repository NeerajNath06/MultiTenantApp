using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class ShiftsController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? sortBy = null, string? sortDirection = "asc")
    {
        return View(new ShiftListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateShiftRequest
        {
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(18, 0, 0)
        });
    }

    public IActionResult Edit(Guid id)
    {
        return View(new UpdateShiftRequest
        {
            Id = id,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(18, 0, 0),
            IsActive = true
        });
    }
}
