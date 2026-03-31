using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class PaymentsController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null, Guid? billId = null, Guid? clientId = null, string? status = null)
    {
        return View(new PaymentListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreatePaymentRequest { PaymentDate = DateTime.UtcNow, Status = "Pending", PaymentMethod = "Cash" });
    }

    public IActionResult Edit(Guid id)
    {
        return View(new UpdatePaymentRequest
        {
            Id = id,
            PaymentMethod = "Cash",
            Status = "Pending"
        });
    }

    public IActionResult Details(Guid id)
    {
        ViewBag.PaymentId = id;
        return View();
    }
}
