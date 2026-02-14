using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class TrainingRecordsController : Controller
{
    private readonly IApiClient _apiClient;

    public TrainingRecordsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null, Guid? guardId = null, string? trainingType = null)
    {
        var query = new Dictionary<string, string?>
        {
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["includeInactive"] = "false"
        };
        if (!string.IsNullOrEmpty(search)) query["search"] = search;
        if (guardId.HasValue) query["guardId"] = guardId.Value.ToString();
        if (!string.IsNullOrEmpty(trainingType)) query["trainingType"] = trainingType;
        var result = await _apiClient.GetAsync<TrainingRecordListResponse>("api/v1/TrainingRecords", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new TrainingRecordListResponse());
    }

    public async Task<IActionResult> Create()
    {
        var guardResult = await _apiClient.GetAsync<GuardListResponse>("api/v1/SecurityGuards", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Guards = guardResult.Data?.Items ?? new List<GuardItemDto>();
        return View(new CreateTrainingRecordRequest { TrainingDate = DateTime.UtcNow, Status = "Completed" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTrainingRecordRequest request)
    {
        if (!ModelState.IsValid)
        {
            var guardResult = await _apiClient.GetAsync<GuardListResponse>("api/v1/SecurityGuards", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
            ViewBag.Guards = guardResult.Data?.Items ?? new List<GuardItemDto>();
            return View(request);
        }
        var body = new
        {
            guardId = request.GuardId,
            trainingType = request.TrainingType,
            trainingName = request.TrainingName,
            trainingProvider = request.TrainingProvider,
            trainingDate = request.TrainingDate,
            expiryDate = request.ExpiryDate,
            status = request.Status,
            certificateNumber = request.CertificateNumber,
            score = request.Score,
            remarks = request.Remarks
        };
        var result = await _apiClient.PostAsync<Guid>("api/v1/TrainingRecords", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Training record created successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        var guardRes = await _apiClient.GetAsync<GuardListResponse>("api/v1/SecurityGuards", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Guards = guardRes.Data?.Items ?? new List<GuardItemDto>();
        return View(request);
    }
}
