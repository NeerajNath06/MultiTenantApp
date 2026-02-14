using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class ExpensesController : Controller
{
    private readonly IApiClient _apiClient;

    public ExpensesController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? category = null, string? status = null)
    {
        var query = new Dictionary<string, string?>
        {
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["includeInactive"] = "false"
        };
        if (!string.IsNullOrEmpty(search)) query["search"] = search;
        if (!string.IsNullOrEmpty(category)) query["category"] = category;
        if (!string.IsNullOrEmpty(status)) query["status"] = status;
        var result = await _apiClient.GetAsync<ExpenseListResponse>("api/v1/Expenses", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new ExpenseListResponse());
    }

    public async Task<IActionResult> Create()
    {
        await LoadDropdowns();
        return View(new CreateExpenseRequest { ExpenseDate = DateTime.UtcNow, Status = "Pending", PaymentMethod = "Cash" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateExpenseRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(request);
        }
        var body = new
        {
            expenseNumber = request.ExpenseNumber,
            expenseDate = request.ExpenseDate,
            category = request.Category,
            description = request.Description,
            amount = request.Amount,
            paymentMethod = request.PaymentMethod,
            vendorName = request.VendorName,
            receiptNumber = request.ReceiptNumber,
            siteId = request.SiteId,
            guardId = request.GuardId,
            status = request.Status,
            notes = request.Notes
        };
        var result = await _apiClient.PostAsync<Guid>("api/v1/Expenses", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Expense created successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        await LoadDropdowns();
        return View(request);
    }

    private async Task LoadDropdowns()
    {
        var siteResult = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Sites = siteResult.Data?.Items ?? new List<SiteDto>();
        var guardResult = await _apiClient.GetAsync<GuardListResponse>("api/v1/SecurityGuards", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Guards = guardResult.Data?.Items ?? new List<GuardItemDto>();
    }
}
