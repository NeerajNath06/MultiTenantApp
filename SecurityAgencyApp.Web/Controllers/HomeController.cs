using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IApiClient _apiClient;

    public HomeController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _apiClient.GetAsync<DashboardDataDto>("api/v1/Dashboard");
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new DashboardDataDto());
    }
}
