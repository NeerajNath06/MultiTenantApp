using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class SitePostsController : Controller
{
    private readonly IApiClient _apiClient;

    public SitePostsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null, Guid? siteId = null, Guid? branchId = null)
    {
        await LoadSiteAndBranchFiltersAsync(siteId, branchId);

        var result = await _apiClient.GetAsync<SitePostListResponse>("api/v1/SitePosts", new Dictionary<string, string?>
        {
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["search"] = string.IsNullOrWhiteSpace(search) ? null : search,
            ["siteId"] = siteId?.ToString(),
            ["branchId"] = branchId?.ToString(),
            ["includeInactive"] = "true"
        });

        ViewBag.Search = search;
        return View(result.Success && result.Data != null ? result.Data : new SitePostListResponse());
    }

    public async Task<IActionResult> Create()
    {
        await LoadSiteAndBranchFiltersAsync(null, null);
        return View(new CreateSitePostRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSitePostRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadSiteAndBranchFiltersAsync(request.SiteId, request.BranchId);
            return View(request);
        }

        var result = await _apiClient.PostAsync<Guid>("api/v1/SitePosts", request);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Site post created successfully.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, result.Message);
        await LoadSiteAndBranchFiltersAsync(request.SiteId, request.BranchId);
        return View(request);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _apiClient.GetAsync<SitePostItemDto>($"api/v1/SitePosts/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();

        var dto = result.Data;
        await LoadSiteAndBranchFiltersAsync(dto.SiteId, dto.BranchId);
        return View(new UpdateSitePostRequest
        {
            Id = dto.Id,
            SiteId = dto.SiteId,
            BranchId = dto.BranchId,
            PostCode = dto.PostCode,
            PostName = dto.PostName,
            ShiftName = dto.ShiftName,
            SanctionedStrength = dto.SanctionedStrength,
            GenderRequirement = dto.GenderRequirement,
            SkillRequirement = dto.SkillRequirement,
            RequiresWeapon = dto.RequiresWeapon,
            RelieverRequired = dto.RelieverRequired,
            WeeklyOffPattern = dto.WeeklyOffPattern,
            IsActive = dto.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateSitePostRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadSiteAndBranchFiltersAsync(request.SiteId, request.BranchId);
            return View(request);
        }

        var result = await _apiClient.PutAsync<object>($"api/v1/SitePosts/{request.Id}", request);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Site post updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, result.Message);
        await LoadSiteAndBranchFiltersAsync(request.SiteId, request.BranchId);
        return View(request);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _apiClient.GetAsync<SitePostItemDto>($"api/v1/SitePosts/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();

        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _apiClient.DeleteAsync($"api/v1/SitePosts/{id}");
        if (result.Success)
            TempData["SuccessMessage"] = "Site post deleted successfully.";
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSiteAndBranchFiltersAsync(Guid? siteId, Guid? branchId)
    {
        var siteResult = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", new Dictionary<string, string?>
        {
            ["pageSize"] = "1000",
            ["includeInactive"] = "false"
        });
        var sites = siteResult.Success && siteResult.Data?.Items != null
            ? siteResult.Data.Items.OrderBy(s => s.SiteName).ToList()
            : new List<SiteDto>();

        var branchResult = await _apiClient.GetAsync<BranchListResponse>("api/v1/Branches", new Dictionary<string, string?>
        {
            ["includeInactive"] = "false"
        });
        var branches = branchResult.Success && branchResult.Data?.Items != null
            ? branchResult.Data.Items.OrderBy(b => b.BranchName).ToList()
            : new List<BranchDto>();

        ViewBag.Sites = new SelectList(sites, nameof(SiteDto.Id), nameof(SiteDto.SiteName), siteId);
        ViewBag.Branches = new SelectList(branches, nameof(BranchDto.Id), nameof(BranchDto.BranchName), branchId);
    }
}
