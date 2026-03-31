using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class DesignationsController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? sortBy = null, string? sortDirection = "asc")
    {
        return View(new DesignationListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateDesignationRequest());
    }

    public IActionResult Edit(Guid id)
    {
        return View(new UpdateDesignationRequest { Id = id, IsActive = true });
    }
}
