using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class BranchesController : Controller
{
    private readonly IApiClient _apiClient;

    public BranchesController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(string? search = null)
    {
        var result = await _apiClient.GetAsync<BranchListResponse>("api/v1/Branches", new Dictionary<string, string?>
        {
            ["includeInactive"] = "true",
            ["search"] = string.IsNullOrWhiteSpace(search) ? null : search
        });

        ViewBag.Search = search;
        return View(result.Success && result.Data != null ? result.Data : new BranchListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateBranchRequest { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBranchRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _apiClient.PostAsync<Guid>("api/v1/Branches", request);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Branch created successfully.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, result.Message);
        return View(request);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _apiClient.GetAsync<BranchDto>($"api/v1/Branches/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();

        var branch = result.Data;
        return View(new UpdateBranchRequest
        {
            Id = branch.Id,
            BranchCode = branch.BranchCode,
            BranchName = branch.BranchName,
            Address = branch.Address,
            City = branch.City,
            State = branch.State,
            PinCode = branch.PinCode,
            ContactPerson = branch.ContactPerson,
            ContactPhone = branch.ContactPhone,
            ContactEmail = branch.ContactEmail,
            GstNumber = branch.GstNumber,
            LabourLicenseNumber = branch.LabourLicenseNumber,
            NumberPrefix = branch.NumberPrefix,
            IsHeadOffice = branch.IsHeadOffice,
            IsActive = branch.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateBranchRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _apiClient.PutAsync<object>($"api/v1/Branches/{request.Id}", request);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Branch updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, result.Message);
        return View(request);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _apiClient.GetAsync<BranchDto>($"api/v1/Branches/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();

        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _apiClient.DeleteAsync($"api/v1/Branches/{id}");
        if (result.Success)
            TempData["SuccessMessage"] = "Branch deleted successfully.";
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index));
    }
}
