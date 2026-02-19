using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class CompanyProfileController : Controller
{
    private readonly IApiClient _apiClient;

    public CompanyProfileController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var profileResult = await _apiClient.GetAsync<TenantProfileDto>("api/v1/TenantProfile");
        var profile = profileResult.Success && profileResult.Data != null ? profileResult.Data : new TenantProfileDto();

        var docsResult = await _apiClient.GetAsync<List<TenantDocumentDto>>("api/v1/TenantDocuments");
        var documents = docsResult.Success && docsResult.Data != null ? docsResult.Data : new List<TenantDocumentDto>();

        ViewBag.Profile = profile;
        ViewBag.Documents = documents;
        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateTenantProfileRequest request)
    {
        if (request == null)
            return RedirectToAction(nameof(Index));
        if (string.IsNullOrWhiteSpace(request.CompanyName) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Phone))
        {
            TempData["ErrorMessage"] = "Company name, email and phone are required.";
            return RedirectToAction(nameof(Index));
        }
        var result = await _apiClient.PutAsync<bool>("api/v1/TenantProfile", request);
        if (result.Success)
            TempData["SuccessMessage"] = "Profile updated successfully.";
        else
            TempData["ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocument(string documentType, string? documentNumber, DateTime? expiryDate, IFormFile? file)
    {
        if (string.IsNullOrWhiteSpace(documentType) || file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Document type and file are required.";
            return RedirectToAction(nameof(Index));
        }
        using var stream = file.OpenReadStream();
        var formData = new Dictionary<string, object>
        {
            ["documentType"] = documentType,
            ["file"] = new MultipartFile
            {
                Content = stream,
                FileName = file.FileName,
                ContentType = file.ContentType
            }
        };
        if (!string.IsNullOrWhiteSpace(documentNumber))
            formData["documentNumber"] = documentNumber;
        if (expiryDate.HasValue)
            formData["expiryDate"] = expiryDate.Value.ToString("O");

        var result = await _apiClient.PostMultipartAsync<Guid>("api/v1/TenantDocuments", formData);
        if (result.Success)
            TempData["SuccessMessage"] = "Document uploaded successfully.";
        else
            TempData["ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Export company documents list (metadata) as CSV.</summary>
    [HttpGet]
    public async Task<IActionResult> ExportDocuments(string format = "csv")
    {
        var result = await _apiClient.GetFileAsync("api/v1/TenantDocuments/export", new Dictionary<string, string?> { ["format"] = format ?? "csv" });
        if (!result.Success || result.Data == null)
            return NotFound();
        return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
    }

    public async Task<IActionResult> DownloadDocument(Guid id)
    {
        var result = await _apiClient.GetFileAsync($"api/v1/TenantDocuments/{id}/download");
        if (!result.Success || result.Data == null)
            return NotFound();
        return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocument([FromForm] Guid id)
    {
        var result = await _apiClient.DeleteAsync($"api/v1/TenantDocuments/{id}");
        if (result.Success)
            TempData["SuccessMessage"] = "Document deleted.";
        else
            TempData["ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
