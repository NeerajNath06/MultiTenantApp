using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class BillsController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? status = null)
    {
        return View(new BillListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateBillRequest { BillDate = DateTime.UtcNow, Status = "Draft" });
    }

    public IActionResult Edit(Guid id)
    {
        return View(new UpdateBillRequest
        {
            Id = id,
            Status = "Draft",
            Items = new List<CreateBillLineItemRequest>()
        });
    }

    public IActionResult Details(Guid id)
    {
        ViewBag.BillId = id;
        return View();
    }
}
