using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class ContractsController : Controller
{
    public IActionResult Index(int pageNumber = 1, int pageSize = 10, string? search = null, Guid? clientId = null, string? status = null)
    {
        return View(new ContractListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateContractRequest { StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddYears(1), Status = "Draft", BillingCycle = "Monthly" });
    }

    public IActionResult Edit(Guid id)
    {
        return View(new UpdateContractRequest
        {
            Id = id,
            BillingCycle = "Monthly",
            Status = "Draft"
        });
    }

    public IActionResult Details(Guid id)
    {
        ViewBag.ContractId = id;
        return View();
    }
}
