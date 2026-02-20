using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly IApiClient _apiClient;

    public ReportsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>Page to download agency monthly Excel (BILL, ATTENDANCE, Wages, Other sheets).</summary>
    public IActionResult Index()
    {
        var now = DateTime.Today;
        ViewBag.Year = now.Year;
        ViewBag.Month = now.Month;
        return View("MonthlyExcel");
    }

    public IActionResult MonthlyExcel()
    {
        return Index();
    }

    /// <summary>Download monthly Excel file from API.</summary>
    [HttpGet]
    public async Task<IActionResult> DownloadMonthlyExcel(int year, int month, Guid? siteId = null, Guid? wageId = null, Guid? billId = null)
    {
        if (month < 1 || month > 12)
        {
            TempData["Error"] = "Invalid month.";
            return RedirectToAction(nameof(MonthlyExcel));
        }
        var query = new Dictionary<string, string?>
        {
            ["year"] = year.ToString(),
            ["month"] = month.ToString()
        };
        if (siteId.HasValue) query["siteId"] = siteId.Value.ToString();
        if (wageId.HasValue) query["wageId"] = wageId.Value.ToString();
        if (billId.HasValue) query["billId"] = billId.Value.ToString();
        var result = await _apiClient.GetFileAsync("api/v1/Reports/monthly-excel", query);
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Message ?? "Failed to generate report.";
            return RedirectToAction(nameof(MonthlyExcel));
        }
        var fileName = $"Agency_Monthly_{year}_{month:D2}.xlsx";
        return File(result.Data.Content, result.Data.ContentType, result.Data.FileName ?? fileName);
    }
}
