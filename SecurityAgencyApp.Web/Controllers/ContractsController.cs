using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class ContractsController : Controller
{
    private readonly IApiClient _apiClient;

    public ContractsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null, Guid? clientId = null, string? status = null)
    {
        var query = new Dictionary<string, string?>
        {
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["includeInactive"] = "false"
        };
        if (!string.IsNullOrEmpty(search)) query["search"] = search;
        if (clientId.HasValue) query["clientId"] = clientId.Value.ToString();
        if (!string.IsNullOrEmpty(status)) query["status"] = status;
        var result = await _apiClient.GetAsync<ContractListResponse>("api/v1/Contracts", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new ContractListResponse());
    }

    public async Task<IActionResult> Create()
    {
        var clientResult = await _apiClient.GetAsync<ClientListResponse>("api/v1/Clients", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Clients = clientResult.Data?.Items ?? new List<ClientItemDto>();
        var siteResult = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Sites = siteResult.Data?.Items ?? new List<SiteDto>();
        return View(new CreateContractRequest { StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddYears(1), Status = "Draft", BillingCycle = "Monthly" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateContractRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(request);
        }
        var body = new
        {
            contractNumber = request.ContractNumber,
            clientId = request.ClientId,
            title = request.Title,
            description = request.Description,
            startDate = request.StartDate,
            endDate = request.EndDate,
            contractValue = request.ContractValue,
            billingCycle = request.BillingCycle,
            monthlyAmount = request.MonthlyAmount,
            status = request.Status,
            termsAndConditions = request.TermsAndConditions,
            paymentTerms = request.PaymentTerms,
            numberOfGuards = request.NumberOfGuards,
            autoRenewal = request.AutoRenewal,
            renewalDate = request.RenewalDate,
            notes = request.Notes,
            sites = (request.Sites ?? new List<ContractSiteItemDto>()).Select(s => new { siteId = s.SiteId, requiredGuards = s.RequiredGuards, shiftDetails = s.ShiftDetails })
        };
        var result = await _apiClient.PostAsync<Guid>("api/v1/Contracts", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Contract created successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        await LoadDropdowns();
        return View(request);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _apiClient.GetAsync<ContractDetailDto>($"api/v1/Contracts/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();
        var clientResult = await _apiClient.GetAsync<ClientListResponse>("api/v1/Clients", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Clients = clientResult.Data?.Items ?? new List<ClientItemDto>();
        var d = result.Data;
        return View(new UpdateContractRequest
        {
            Id = d.Id,
            ContractNumber = d.ContractNumber,
            ClientId = d.ClientId,
            Title = d.Title,
            Description = d.Description,
            StartDate = d.StartDate,
            EndDate = d.EndDate,
            ContractValue = d.ContractValue,
            BillingCycle = d.BillingCycle ?? "Monthly",
            MonthlyAmount = d.MonthlyAmount,
            Status = d.Status ?? "Draft",
            TermsAndConditions = d.TermsAndConditions,
            PaymentTerms = d.PaymentTerms,
            NumberOfGuards = d.NumberOfGuards,
            AutoRenewal = d.AutoRenewal,
            RenewalDate = d.RenewalDate,
            Notes = d.Notes
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateContractRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);
        var body = new
        {
            id = request.Id,
            contractNumber = request.ContractNumber,
            clientId = request.ClientId,
            title = request.Title,
            description = request.Description,
            startDate = request.StartDate,
            endDate = request.EndDate,
            contractValue = request.ContractValue,
            billingCycle = request.BillingCycle,
            monthlyAmount = request.MonthlyAmount,
            status = request.Status,
            termsAndConditions = request.TermsAndConditions,
            paymentTerms = request.PaymentTerms,
            numberOfGuards = request.NumberOfGuards,
            autoRenewal = request.AutoRenewal,
            renewalDate = request.RenewalDate,
            notes = request.Notes
        };
        var result = await _apiClient.PutAsync<object>($"api/v1/Contracts/{request.Id}", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Contract updated successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        var clientResult = await _apiClient.GetAsync<ClientListResponse>("api/v1/Clients", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Clients = clientResult.Data?.Items ?? new List<ClientItemDto>();
        return View(request);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _apiClient.GetAsync<ContractDetailDto>($"api/v1/Contracts/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();
        return View(result.Data);
    }

    private async Task LoadDropdowns()
    {
        var clientResult = await _apiClient.GetAsync<ClientListResponse>("api/v1/Clients", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Clients = clientResult.Data?.Items ?? new List<ClientItemDto>();
        var siteResult = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Sites = siteResult.Data?.Items ?? new List<SiteDto>();
    }
}
