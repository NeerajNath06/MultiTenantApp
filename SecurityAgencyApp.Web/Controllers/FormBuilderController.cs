using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class FormBuilderController : Controller
{
    private readonly IApiClient _apiClient;

    public FormBuilderController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(bool includeInactive = false)
    {
        var query = new Dictionary<string, string?> { ["includeInactive"] = includeInactive.ToString().ToLowerInvariant() };
        var result = await _apiClient.GetAsync<FormTemplateListResponse>("api/v1/FormBuilder/templates", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new FormTemplateListResponse());
    }

    public IActionResult Create()
    {
        return View(new CreateFormTemplateRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateFormTemplateRequest request)
    {
        if (request.Fields == null || request.Fields.Count == 0)
            ModelState.AddModelError("", "At least one form field is required");
        if (!ModelState.IsValid)
            return View(request);
        var body = new { name = request.Name, code = request.Code, description = request.Description, category = request.Category, isSystemTemplate = request.IsSystemTemplate, fields = (request.Fields ?? new List<CreateFormFieldRequest>()).Select(f => new { fieldName = f.FieldName, fieldLabel = f.FieldLabel, fieldType = f.FieldType, fieldOrder = f.FieldOrder, isRequired = f.IsRequired, defaultValue = f.DefaultValue, placeholder = f.Placeholder, validationRules = f.ValidationRules, options = f.Options }) };
        var result = await _apiClient.PostAsync<Guid>("api/v1/FormBuilder/templates", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Form template created successfully!";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        return View(request);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _apiClient.GetAsync<FormTemplateDetailDto>($"api/v1/FormBuilder/templates/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();
        return View(result.Data);
    }

    public async Task<IActionResult> Submit(Guid id)
    {
        var result = await _apiClient.GetAsync<FormTemplateDetailDto>($"api/v1/FormBuilder/templates/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();
        ViewBag.Template = result.Data;
        return View(new SubmitFormRequest { FormTemplateId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitFormRequest request)
    {
        if (ModelState.IsValid)
        {
            var body = new { formTemplateId = request.FormTemplateId, data = request.Data ?? new Dictionary<string, object>(), remarks = request.Remarks };
            var result = await _apiClient.PostAsync<Guid>("api/v1/FormBuilder/submit", body);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Form submitted successfully!";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Message);
        }
        var templateResult = await _apiClient.GetAsync<FormTemplateDetailDto>($"api/v1/FormBuilder/templates/{request.FormTemplateId}");
        if (templateResult.Success && templateResult.Data != null)
            ViewBag.Template = templateResult.Data;
        return View(request);
    }
}
