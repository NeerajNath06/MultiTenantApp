using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Model.Api;
using SecurityAgencyApp.Web.Services;
using SecurityAgencyApp.Application.Features.MonthlyDocuments.Commands.GenerateMonthlyDocuments;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class SitesController : Controller
{
    private readonly IApiClient _apiClient;

    public SitesController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? sortBy = null, string? sortDirection = "asc")
    {
        var query = new Dictionary<string, string?>
        {
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["includeInactive"] = "false"
        };
        if (!string.IsNullOrEmpty(search)) query["search"] = search;
        if (!string.IsNullOrEmpty(sortBy)) query["sortBy"] = sortBy;
        if (!string.IsNullOrEmpty(sortDirection)) query["sortDirection"] = sortDirection;
        var roles = HttpContext.Session.GetString("Roles") ?? "";
        if (roles.Contains("SUPERVISOR", StringComparison.OrdinalIgnoreCase))
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (Guid.TryParse(userIdStr, out var currentUserId))
                query["supervisorId"] = currentUserId.ToString();
        }

        var result = await _apiClient.GetAsync<SiteListResponse>("api/v1/Sites", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new SiteListResponse());
    }

    public async Task<IActionResult> Create()
    {
        await LoadSupervisorsIntoViewBag(null);
        await LoadBranchesIntoViewBag(null);
        return View(new CreateSiteRequest { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSiteRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadSupervisorsIntoViewBag(request.SupervisorIds);
            await LoadBranchesIntoViewBag(request.BranchId);
            return View(request);
        }

        var result = await _apiClient.PostAsync<Guid>("api/v1/Sites", new
        {
            siteCode = request.SiteCode,
            siteName = request.SiteName,
            clientName = request.ClientName,
            address = request.Address,
            city = request.City,
            state = request.State,
            pinCode = request.PinCode,
            contactPerson = request.ContactPerson ?? "",
            contactPhone = request.ContactPhone ?? "",
            contactEmail = request.ContactEmail,
            branchId = request.BranchId,
            emergencyContactName = request.EmergencyContactName,
            emergencyContactPhone = request.EmergencyContactPhone,
            musterPoint = request.MusterPoint,
            accessZoneNotes = request.AccessZoneNotes,
            siteInstructionBook = request.SiteInstructionBook,
            geofenceExceptionNotes = request.GeofenceExceptionNotes,
            isActive = request.IsActive,
            latitude = request.Latitude,
            longitude = request.Longitude,
            geofenceRadiusMeters = request.GeofenceRadiusMeters,
            posts = request.Posts ?? new List<SitePostDto>(),
            deploymentPlan = request.ActiveDeploymentPlan,
            supervisorIds = request.SupervisorIds ?? new List<Guid>()
        });

        if (result.Success)
        {
            TempData["SuccessMessage"] = "Site created successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        await LoadSupervisorsIntoViewBag(request.SupervisorIds);
        await LoadBranchesIntoViewBag(request.BranchId);
        return View(request);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _apiClient.GetAsync<SiteDto>($"api/v1/Sites/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();

        var d = result.Data;
        await LoadSupervisorsIntoViewBag(d.SupervisorIds);
        await LoadBranchesIntoViewBag(d.BranchId);
        await LoadEditDependenciesAsync(id);
        return View(new UpdateSiteRequest
        {
            Id = d.Id,
            SiteCode = d.SiteCode,
            SiteName = d.SiteName,
            ClientId = d.ClientId,
            BranchId = d.BranchId,
            ClientName = d.ClientName,
            Address = d.Address,
            City = d.City,
            State = d.State,
            PinCode = d.PinCode,
            ContactPerson = d.ContactPerson,
            ContactPhone = d.ContactPhone,
            ContactEmail = d.ContactEmail,
            EmergencyContactName = d.EmergencyContactName,
            EmergencyContactPhone = d.EmergencyContactPhone,
            MusterPoint = d.MusterPoint,
            AccessZoneNotes = d.AccessZoneNotes,
            SiteInstructionBook = d.SiteInstructionBook,
            GeofenceExceptionNotes = d.GeofenceExceptionNotes,
            IsActive = d.IsActive,
            Latitude = d.Latitude,
            Longitude = d.Longitude,
            GeofenceRadiusMeters = d.GeofenceRadiusMeters,
            Posts = d.Posts ?? new List<SitePostDto>(),
            ActiveDeploymentPlan = d.ActiveDeploymentPlan,
            SupervisorIds = d.SupervisorIds ?? new List<Guid>()
        });
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _apiClient.GetAsync<SiteDto>($"api/v1/Sites/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();

        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveRate(Guid siteId, Guid clientId, decimal rateAmount, DateTime effectiveFrom, Guid? ratePlanId = null, decimal? epfPercent = null, decimal? esicPercent = null, decimal? allowancePercent = null, decimal? epfWageCap = null)
    {
        var res = await _apiClient.PostAsync<Guid>("api/v1/SiteRates", new UpsertSiteRateRequest
        {
            Id = ratePlanId,
            SiteId = siteId,
            ClientId = clientId,
            RateAmount = rateAmount,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = null,
            EpfPercent = epfPercent,
            EsicPercent = esicPercent,
            AllowancePercent = allowancePercent,
            EpfWageCap = epfWageCap
        });
        if (res.Success)
            TempData["SuccessMessage"] = ratePlanId.HasValue ? "Rate plan updated." : "Site rate saved.";
        else
            TempData["Error"] = res.Message;
        return RedirectToAction(nameof(Edit), new { id = siteId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRate(Guid id, Guid siteId)
    {
        var res = await _apiClient.DeleteAsync($"api/v1/SiteRates/{id}");
        if (res.Success)
            TempData["SuccessMessage"] = "Rate plan deleted.";
        else
            TempData["Error"] = res.Message;
        return RedirectToAction(nameof(Edit), new { id = siteId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateSiteRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadSupervisorsIntoViewBag(request.SupervisorIds);
            await LoadBranchesIntoViewBag(request.BranchId);
            await LoadEditDependenciesAsync(request.Id);
            return View(request);
        }

        var result = await _apiClient.PutAsync<object>($"api/v1/Sites/{request.Id}", new
        {
            id = request.Id,
            siteCode = request.SiteCode,
            siteName = request.SiteName,
            clientId = request.ClientId,
            clientName = request.ClientName,
            address = request.Address,
            city = request.City,
            state = request.State,
            pinCode = request.PinCode,
            contactPerson = request.ContactPerson ?? "",
            contactPhone = request.ContactPhone ?? "",
            contactEmail = request.ContactEmail,
            branchId = request.BranchId,
            emergencyContactName = request.EmergencyContactName,
            emergencyContactPhone = request.EmergencyContactPhone,
            musterPoint = request.MusterPoint,
            accessZoneNotes = request.AccessZoneNotes,
            siteInstructionBook = request.SiteInstructionBook,
            geofenceExceptionNotes = request.GeofenceExceptionNotes,
            isActive = request.IsActive,
            latitude = request.Latitude,
            longitude = request.Longitude,
            geofenceRadiusMeters = request.GeofenceRadiusMeters,
            posts = request.Posts ?? new List<SitePostDto>(),
            deploymentPlan = request.ActiveDeploymentPlan,
            supervisorIds = request.SupervisorIds ?? new List<Guid>()
        });

        if (result.Success)
        {
            TempData["SuccessMessage"] = "Site updated successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        await LoadSupervisorsIntoViewBag(request.SupervisorIds);
        await LoadBranchesIntoViewBag(request.BranchId);
        await LoadEditDependenciesAsync(request.Id);
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _apiClient.DeleteAsync($"api/v1/Sites/{id}");
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Site deleted successfully";
            return RedirectToAction(nameof(Index));
        }
        TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Proxy for monthly report download (calls API and returns file).</summary>
    [HttpGet]
    [Route("Sites/DownloadReport", Name = "DownloadReport")]
    public async Task<IActionResult> DownloadReport(Guid siteId, int year, int month, string format, string reportType)
    {
        var path = reportType?.ToLowerInvariant() switch
        {
            "bill" => "api/report/generate-bill",
            "attendance" => "api/report/generate-attendance",
            "wages" => "api/report/generate-wages",
            "full" => "api/report/generate-full-report",
            _ => null
        };
        if (path == null)
        {
            Response.StatusCode = 400;
            return Content("Invalid report type.", "text/plain");
        }
        if (month < 1 || month > 12)
        {
            Response.StatusCode = 400;
            return Content("Invalid month.", "text/plain");
        }
        var query = new Dictionary<string, string?>
        {
            ["siteId"] = siteId.ToString(),
            ["year"] = year.ToString(),
            ["month"] = month.ToString(),
            ["format"] = (format ?? "Excel").Trim()
        };
        var result = await _apiClient.GetFileAsync(path, query);
        if (!result.Success || result.Data == null)
        {
            var message = result.Message ?? "Report not available.";
            Response.StatusCode = 404;
            return Content(message, "text/plain");
        }
        return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
    }

    /// <summary>Generate Bill + Wage for a site/month and return IDs for tracking.</summary>
    [HttpGet]
    [Route("Sites/GenerateMonthlyTracking", Name = "GenerateMonthlyTracking")]
    public async Task<IActionResult> GenerateMonthlyTracking(Guid siteId, int year, int month)
    {
        var res = await _apiClient.PostAsync<GenerateMonthlyDocumentsResultDto>(
            "api/v1/MonthlyDocuments/generate-all",
            new { siteId, year, month });
        if (!res.Success || res.Data == null)
        {
            Response.StatusCode = 400;
            return Content(res.Message ?? "Generation failed.", "text/plain");
        }
        return Json(res.Data);
    }

    private async Task LoadSupervisorsIntoViewBag(IList<Guid>? selectedIds = null)
    {
        var res = await _apiClient.GetAsync<UserListResponse>("api/v1/Supervisors", new Dictionary<string, string?> { ["pageSize"] = "500", ["isActive"] = "true" });
        var list = res.Success && res.Data?.Items != null ? res.Data.Items : new List<UserItemDto>();
        var items = list.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = string.IsNullOrWhiteSpace($"{s.FirstName} {s.LastName}".Trim()) ? s.UserName : $"{s.FirstName} {s.LastName}".Trim()
        }).ToList();
        ViewBag.Supervisors = items;
    }

    private async Task LoadBranchesIntoViewBag(Guid? selectedBranchId)
    {
        var res = await _apiClient.GetAsync<BranchListResponse>("api/v1/Branches", new Dictionary<string, string?>
        {
            ["includeInactive"] = "false"
        });

        var branches = res.Success && res.Data?.Items != null
            ? res.Data.Items.OrderBy(b => b.BranchName).ToList()
            : new List<BranchDto>();

        ViewBag.Branches = new SelectList(branches, nameof(BranchDto.Id), nameof(BranchDto.BranchName), selectedBranchId);
    }

    private async Task LoadEditDependenciesAsync(Guid siteId)
    {
        var clientResult = await _apiClient.GetAsync<ClientListResponse>("api/v1/Clients", new Dictionary<string, string?>
        {
            ["includeInactive"] = "false",
            ["pageSize"] = "1000",
            ["siteId"] = siteId.ToString()
        });
        ViewBag.Clients = clientResult.Data?.Items ?? new List<ClientItemDto>();

        var rateRes = await _apiClient.GetAsync<SiteRateDto>($"api/v1/SiteRates/{siteId}", new Dictionary<string, string?> { ["asOfDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd") });
        ViewBag.SiteRate = rateRes.Success ? rateRes.Data : null;
    }
}
