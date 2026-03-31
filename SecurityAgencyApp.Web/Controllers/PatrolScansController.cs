using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class PatrolScansController : Controller
{
    public IActionResult Index(Guid? guardId = null, Guid? siteId = null, DateTime? dateFrom = null, DateTime? dateTo = null, int pageNumber = 1, int pageSize = 20)
    {
        return View(new PatrolScanListResponse { Items = new List<PatrolScanItemDto>() });
    }
}
