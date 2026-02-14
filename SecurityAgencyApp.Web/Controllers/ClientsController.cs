using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class ClientsController : Controller
{
    private readonly IApiClient _apiClient;

    public ClientsController(IApiClient apiClient)
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
        var result = await _apiClient.GetAsync<ClientListResponse>("api/v1/Clients", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new ClientListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateClientRequest { Status = "Active" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateClientRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);
        var body = new
        {
            clientCode = request.ClientCode,
            companyName = request.CompanyName,
            contactPerson = request.ContactPerson,
            email = request.Email,
            phoneNumber = request.PhoneNumber,
            alternatePhone = request.AlternatePhone,
            address = request.Address,
            city = request.City,
            state = request.State,
            pinCode = request.PinCode,
            gstNumber = request.GSTNumber,
            panNumber = request.PANNumber,
            website = request.Website,
            status = request.Status,
            notes = request.Notes
        };
        var result = await _apiClient.PostAsync<Guid>("api/v1/Clients", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Client created successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        return View(request);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _apiClient.GetAsync<ClientDetailDto>($"api/v1/Clients/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();
        var d = result.Data;
        return View(new UpdateClientRequest
        {
            Id = d.Id,
            ClientCode = d.ClientCode,
            CompanyName = d.CompanyName,
            ContactPerson = d.ContactPerson,
            Email = d.Email,
            PhoneNumber = d.PhoneNumber,
            AlternatePhone = d.AlternatePhone,
            Address = d.Address ?? "",
            City = d.City ?? "",
            State = d.State ?? "",
            PinCode = d.PinCode ?? "",
            GSTNumber = d.GSTNumber,
            PANNumber = d.PANNumber,
            Website = d.Website,
            Status = d.Status ?? "Active",
            Notes = d.Notes
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateClientRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);
        var body = new
        {
            id = request.Id,
            clientCode = request.ClientCode,
            companyName = request.CompanyName,
            contactPerson = request.ContactPerson,
            email = request.Email,
            phoneNumber = request.PhoneNumber,
            alternatePhone = request.AlternatePhone,
            address = request.Address,
            city = request.City,
            state = request.State,
            pinCode = request.PinCode,
            gstNumber = request.GSTNumber,
            panNumber = request.PANNumber,
            website = request.Website,
            status = request.Status,
            notes = request.Notes
        };
        var result = await _apiClient.PutAsync<object>($"api/v1/Clients/{request.Id}", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Client updated successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        return View(request);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _apiClient.GetAsync<ClientDetailDto>($"api/v1/Clients/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();
        return View(result.Data);
    }
}
