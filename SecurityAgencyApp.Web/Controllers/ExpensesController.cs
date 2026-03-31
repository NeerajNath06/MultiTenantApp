using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class ExpensesController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? category = null, string? status = null)
    {
        return View(new ExpenseListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateExpenseRequest { ExpenseDate = DateTime.UtcNow, Status = "Pending", PaymentMethod = "Cash" });
    }

    public IActionResult Details(Guid id)
    {
        ViewBag.ExpenseId = id;
        return View();
    }
}
