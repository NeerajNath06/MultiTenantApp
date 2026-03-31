using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class DepartmentsController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? sortBy = null, string? sortDirection = "asc")
    {
        return View(new DepartmentListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateDepartmentRequest());
    }

    public IActionResult Edit(Guid id)
    {
        return View(new UpdateDepartmentRequest
        {
            Id = id,
            IsActive = true
        });
    }
}
