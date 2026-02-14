using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class BillsController : Controller
{
    private readonly IApiClient _apiClient;

    public BillsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? status = null)
    {
        var query = new Dictionary<string, string?>
        {
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["includeInactive"] = "false"
        };
        if (!string.IsNullOrEmpty(search)) query["search"] = search;
        if (!string.IsNullOrEmpty(status)) query["status"] = status;
        var result = await _apiClient.GetAsync<BillListResponse>("api/v1/Bills", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new BillListResponse());
    }

    public async Task<IActionResult> Create()
    {
        var siteResult = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Sites = siteResult.Data?.Items ?? new List<SiteDto>();
        return View(new CreateBillRequest { BillDate = DateTime.UtcNow, Status = "Draft" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBillRequest request)
    {
        if (!ModelState.IsValid)
        {
            var siteResult = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
            ViewBag.Sites = siteResult.Data?.Items ?? new List<SiteDto>();
            return View(request);
        }
        var body = new
        {
            billNumber = request.BillNumber,
            billDate = request.BillDate,
            siteId = request.SiteId,
            clientName = request.ClientName,
            description = request.Description,
            taxAmount = request.TaxAmount,
            discountAmount = request.DiscountAmount,
            paymentTerms = request.PaymentTerms,
            dueDate = request.DueDate,
            status = request.Status,
            notes = request.Notes,
            items = (request.Items ?? new List<CreateBillLineItemRequest>()).Select(i => new { itemName = i.ItemName, description = i.Description, quantity = i.Quantity, unitPrice = i.UnitPrice, taxRate = i.TaxRate, discountAmount = i.DiscountAmount })
        };
        var result = await _apiClient.PostAsync<Guid>("api/v1/Bills", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Bill created successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        var siteRes = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Sites = siteRes.Data?.Items ?? new List<SiteDto>();
        return View(request);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _apiClient.GetAsync<BillDetailDto>($"api/v1/Bills/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();
        var d = result.Data;
        var request = new UpdateBillRequest
        {
            Id = d.Id,
            BillNumber = d.BillNumber,
            BillDate = d.BillDate,
            SiteId = d.SiteId,
            ClientId = d.ClientId,
            ClientName = d.ClientName,
            Description = d.Description,
            SubTotal = d.SubTotal,
            TaxAmount = d.TaxAmount,
            DiscountAmount = d.DiscountAmount,
            TotalAmount = d.TotalAmount,
            PaymentTerms = d.PaymentTerms,
            DueDate = d.DueDate,
            Status = d.Status,
            Notes = d.Notes,
            Items = d.Items?.Select(i => new CreateBillLineItemRequest { ItemName = i.ItemName, Description = i.Description, Quantity = i.Quantity, UnitPrice = i.UnitPrice, TaxRate = i.TaxRate, DiscountAmount = i.DiscountAmount }).ToList() ?? new List<CreateBillLineItemRequest>()
        };
        var siteResult = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Sites = siteResult.Data?.Items ?? new List<SiteDto>();
        var clientResult = await _apiClient.GetAsync<ClientListResponse>("api/v1/Clients", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Clients = clientResult.Data?.Items ?? new List<ClientItemDto>();
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateBillRequest request)
    {
        if (!ModelState.IsValid)
        {
            var siteResult = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
            ViewBag.Sites = siteResult.Data?.Items ?? new List<SiteDto>();
            var clientResult = await _apiClient.GetAsync<ClientListResponse>("api/v1/Clients", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
            ViewBag.Clients = clientResult.Data?.Items ?? new List<ClientItemDto>();
            return View(request);
        }
        var body = new
        {
            id = request.Id,
            billNumber = request.BillNumber,
            billDate = request.BillDate,
            siteId = request.SiteId,
            clientId = request.ClientId,
            clientName = request.ClientName,
            description = request.Description,
            taxAmount = request.TaxAmount,
            discountAmount = request.DiscountAmount,
            paymentTerms = request.PaymentTerms,
            dueDate = request.DueDate,
            status = request.Status,
            notes = request.Notes,
            items = (request.Items ?? new List<CreateBillLineItemRequest>()).Select(i => new { itemName = i.ItemName, description = i.Description, quantity = i.Quantity, unitPrice = i.UnitPrice, taxRate = i.TaxRate, discountAmount = i.DiscountAmount })
        };
        var result = await _apiClient.PutAsync<object>($"api/v1/Bills/{request.Id}", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Bill updated successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        var siteRes = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Sites = siteRes.Data?.Items ?? new List<SiteDto>();
        var clientRes = await _apiClient.GetAsync<ClientListResponse>("api/v1/Clients", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Clients = clientRes.Data?.Items ?? new List<ClientItemDto>();
        return View(request);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _apiClient.GetAsync<BillDetailDto>($"api/v1/Bills/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();
        return View(result.Data);
    }
}
