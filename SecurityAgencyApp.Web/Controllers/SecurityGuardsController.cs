using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecurityAgencyApp.Domain.Enums;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class SecurityGuardsController : Controller
{
    private readonly IApiClient _apiClient;

    public SecurityGuardsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null, string? sortBy = null, string? sortDirection = "asc", Guid? supervisorId = null)
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
        if (supervisorId.HasValue) query["supervisorId"] = supervisorId.Value.ToString();
        else if (string.Equals(HttpContext.Session.GetString("IsSupervisor"), "True", StringComparison.OrdinalIgnoreCase))
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var userId))
                query["supervisorId"] = userId.ToString();
        }
        var result = await _apiClient.GetAsync<GuardListResponse>("api/v1/SecurityGuards", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new GuardListResponse());
    }

    public async Task<IActionResult> Create()
    {
        await LoadSupervisorsIntoViewBag();
        LoadGenderList();
        return View(new CreateGuardRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateGuardRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadSupervisorsIntoViewBag();
            LoadGenderList();
            return View(request);
        }
        var body = new
        {
            firstName = request.FirstName,
            lastName = request.LastName,
            email = request.Email,
            phoneNumber = request.PhoneNumber,
            alternatePhone = request.AlternatePhone,
            dateOfBirth = request.DateOfBirth,
            gender = request.Gender,
            address = request.Address,
            city = request.City,
            state = request.State,
            pinCode = request.PinCode,
            aadharNumber = request.AadharNumber,
            panNumber = request.PANNumber,
            emergencyContactName = request.EmergencyContactName,
            emergencyContactPhone = request.EmergencyContactPhone,
            joiningDate = request.JoiningDate,
            createLoginAccount = request.CreateLoginAccount,
            loginUserName = request.LoginUserName,
            loginPassword = request.LoginPassword,
            supervisorId = request.SupervisorId
        };
        var result = await _apiClient.PostAsync<Guid>("api/v1/SecurityGuards", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Security guard created successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message ?? "API did not create the guard. Check that the API is running and ApiSettings:BaseUrl points to it.");
        await LoadSupervisorsIntoViewBag();
        LoadGenderList();
        return View(request);
    }

    private void LoadGenderList()
    {
        var items = Enum.GetValues<Gender>().Select(g => new SelectListItem(g.ToString(), g.ToString())).ToList();
        ViewBag.GenderList = new SelectList(items, "Value", "Text");
    }

    private async Task LoadSupervisorsIntoViewBag(Guid? selectedSupervisorId = null)
    {
        var supervisorsRes = await _apiClient.GetAsync<UserListResponse>("api/v1/Supervisors", new Dictionary<string, string?> { ["pageSize"] = "500", ["isActive"] = "true" });
        var supervisors = supervisorsRes.Success && supervisorsRes.Data?.Items != null ? supervisorsRes.Data.Items : new List<UserItemDto>();
        ViewBag.Supervisors = new SelectList(supervisors.Select(s => new { Id = s.Id, Name = string.IsNullOrWhiteSpace($"{s.FirstName} {s.LastName}".Trim()) ? s.UserName : $"{s.FirstName} {s.LastName}".Trim() }), "Id", "Name", selectedSupervisorId);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _apiClient.GetAsync<GuardDetailDto>($"api/v1/SecurityGuards/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();
        var d = result.Data;
        await LoadSupervisorsIntoViewBag(d.SupervisorId);
        LoadGenderList();
        return View(new UpdateGuardRequest
        {
            Id = d.Id,
            GuardCode = d.GuardCode,
            FirstName = d.FirstName,
            LastName = d.LastName,
            Email = d.Email,
            PhoneNumber = d.PhoneNumber ?? "",
            Gender = d.Gender,
            DateOfBirth = d.DateOfBirth,
            Address = d.Address ?? "",
            City = d.City ?? "",
            State = d.State ?? "",
            PinCode = d.PinCode ?? "",
            IsActive = d.IsActive,
            SupervisorId = d.SupervisorId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateGuardRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadSupervisorsIntoViewBag(request.SupervisorId);
            LoadGenderList();
            return View(request);
        }
        var body = new
        {
            id = request.Id,
            guardCode = request.GuardCode,
            firstName = request.FirstName,
            lastName = request.LastName,
            email = request.Email,
            phoneNumber = request.PhoneNumber,
            gender = request.Gender,
            dateOfBirth = request.DateOfBirth,
            address = request.Address,
            city = request.City,
            state = request.State,
            pinCode = request.PinCode,
            isActive = request.IsActive,
            supervisorId = request.SupervisorId
        };
        var result = await _apiClient.PutAsync<object>($"api/v1/SecurityGuards/{request.Id}", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Security guard updated successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        await LoadSupervisorsIntoViewBag(request.SupervisorId);
        LoadGenderList();
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _apiClient.DeleteAsync($"api/v1/SecurityGuards/{id}");
        if (result.Success)
            TempData["SuccessMessage"] = "Security guard deleted successfully";
        else
            TempData["Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
