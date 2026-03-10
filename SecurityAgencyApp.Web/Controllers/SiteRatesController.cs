using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class SiteRatesController : Controller
{
    private readonly IApiClient _apiClient;

    public SiteRatesController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(Guid siteId)
    {
        var siteRes = await _apiClient.GetAsync<SiteDto>($"api/v1/Sites/{siteId}");
        if (!siteRes.Success || siteRes.Data == null)
            return NotFound();

        var histRes = await _apiClient.GetAsync<List<SiteRateHistoryDto>>($"api/v1/SiteRates/{siteId}/history", new Dictionary<string, string?> { ["includeInactive"] = "true" });
        ViewBag.Site = siteRes.Data;
        return View(histRes.Success && histRes.Data != null ? histRes.Data : new List<SiteRateHistoryDto>());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid siteId, Guid clientId, decimal rateAmount, DateTime effectiveFrom, DateTime? effectiveTo)
    {
        var res = await _apiClient.PostAsync<Guid>("api/v1/SiteRates", new UpsertSiteRateRequest
        {
            SiteId = siteId,
            ClientId = clientId,
            RateAmount = rateAmount,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo
        });
        if (res.Success)
            TempData["SuccessMessage"] = "Rate plan saved.";
        else
            TempData["Error"] = res.Message;
        return RedirectToAction(nameof(Index), new { siteId });
    }
}

