using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class SiteRatesController : Controller
{
    public IActionResult Index(Guid siteId)
    {
        ViewBag.SiteId = siteId;
        return View();
    }
}
