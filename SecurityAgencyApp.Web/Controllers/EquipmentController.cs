using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class EquipmentController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? category = null, string? status = null)
    {
        return View(new EquipmentListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateEquipmentRequest { PurchaseDate = DateTime.UtcNow, Status = "Available" });
    }

    public IActionResult Details(Guid id)
    {
        ViewBag.EquipmentId = id;
        return View();
    }
}
