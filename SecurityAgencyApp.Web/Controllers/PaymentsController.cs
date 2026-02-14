using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Web.Filters;
using SecurityAgencyApp.Web.Models.Api;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Controllers;

[Authorize]
public class PaymentsController : Controller
{
    private readonly IApiClient _apiClient;

    public PaymentsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? search = null, Guid? billId = null, Guid? clientId = null, string? status = null)
    {
        var query = new Dictionary<string, string?>
        {
            ["pageNumber"] = pageNumber.ToString(),
            ["pageSize"] = pageSize.ToString(),
            ["includeInactive"] = "false"
        };
        if (!string.IsNullOrEmpty(search)) query["search"] = search;
        if (billId.HasValue) query["billId"] = billId.Value.ToString();
        if (clientId.HasValue) query["clientId"] = clientId.Value.ToString();
        if (!string.IsNullOrEmpty(status)) query["status"] = status;
        var result = await _apiClient.GetAsync<PaymentListResponse>("api/v1/Payments", query);
        if (result.Success && result.Data != null)
            return View(result.Data);
        return View(new PaymentListResponse());
    }

    public async Task<IActionResult> Create()
    {
        await LoadDropdowns();
        return View(new CreatePaymentRequest { PaymentDate = DateTime.UtcNow, Status = "Pending", PaymentMethod = "Cash" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePaymentRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(request);
        }
        var body = new
        {
            paymentNumber = request.PaymentNumber,
            billId = request.BillId,
            clientId = request.ClientId,
            contractId = request.ContractId,
            paymentDate = request.PaymentDate,
            amount = request.Amount,
            paymentMethod = request.PaymentMethod,
            chequeNumber = request.ChequeNumber,
            bankName = request.BankName,
            transactionReference = request.TransactionReference,
            status = request.Status,
            notes = request.Notes,
            receivedDate = request.ReceivedDate
        };
        var result = await _apiClient.PostAsync<Guid>("api/v1/Payments", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Payment created successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        await LoadDropdowns();
        return View(request);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _apiClient.GetAsync<PaymentDetailDto>($"api/v1/Payments/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();
        await LoadDropdowns();
        var d = result.Data;
        return View(new UpdatePaymentRequest
        {
            Id = d.Id,
            PaymentNumber = d.PaymentNumber,
            BillId = d.BillId,
            ClientId = d.ClientId,
            ContractId = d.ContractId,
            PaymentDate = d.PaymentDate,
            Amount = d.Amount,
            PaymentMethod = d.PaymentMethod ?? "Cash",
            ChequeNumber = d.ChequeNumber,
            BankName = d.BankName,
            TransactionReference = d.TransactionReference,
            Status = d.Status ?? "Pending",
            Notes = d.Notes,
            ReceivedDate = d.ReceivedDate
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdatePaymentRequest request)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(request);
        }
        var body = new
        {
            id = request.Id,
            paymentNumber = request.PaymentNumber,
            billId = request.BillId,
            clientId = request.ClientId,
            contractId = request.ContractId,
            paymentDate = request.PaymentDate,
            amount = request.Amount,
            paymentMethod = request.PaymentMethod,
            chequeNumber = request.ChequeNumber,
            bankName = request.BankName,
            transactionReference = request.TransactionReference,
            status = request.Status,
            notes = request.Notes,
            receivedDate = request.ReceivedDate
        };
        var result = await _apiClient.PutAsync<object>($"api/v1/Payments/{request.Id}", body);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Payment updated successfully";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", result.Message);
        await LoadDropdowns();
        return View(request);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _apiClient.GetAsync<PaymentDetailDto>($"api/v1/Payments/{id}");
        if (!result.Success || result.Data == null)
            return NotFound();
        return View(result.Data);
    }

    private async Task LoadDropdowns()
    {
        var billResult = await _apiClient.GetAsync<BillListResponse>("api/v1/Bills", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Bills = billResult.Data?.Items ?? new List<BillItemDto>();
        var clientResult = await _apiClient.GetAsync<ClientListResponse>("api/v1/Clients", new Dictionary<string, string?> { ["includeInactive"] = "false", ["pageSize"] = "1000" });
        ViewBag.Clients = clientResult.Data?.Items ?? new List<ClientItemDto>();
    }
}
