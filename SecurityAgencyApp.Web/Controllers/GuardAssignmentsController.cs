using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class GuardAssignmentsController : Controller
{
    public IActionResult Index(Guid? guardId = null, Guid? siteId = null, int pageNumber = 1, int pageSize = 10, string? search = null, string? sortBy = null, string? sortDirection = "desc")
    {
        return View(new AssignmentListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateAssignmentRequest { AssignmentStartDate = DateTime.Today });
    }
}
