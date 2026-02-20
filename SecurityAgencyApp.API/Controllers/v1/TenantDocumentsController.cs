using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.API.Services;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.TenantDocuments.Commands.CreateTenantDocument;
using SecurityAgencyApp.Application.Features.TenantDocuments.Commands.DeleteTenantDocument;
using SecurityAgencyApp.Application.Features.TenantDocuments.Queries.GetTenantDocumentById;
using SecurityAgencyApp.Application.Features.TenantDocuments.Queries.GetTenantDocuments;
using SecurityAgencyApp.Application.Features.TenantProfile.Queries.GetTenantProfile;
using SecurityAgencyApp.Application.Interfaces;
using TenantDocumentListDto = SecurityAgencyApp.Application.Features.TenantDocuments.Queries.GetTenantDocuments.TenantDocumentDto;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TenantDocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantContext _tenantContext;
    private readonly IWebHostEnvironment _env;

    public TenantDocumentsController(IMediator mediator, ITenantContext tenantContext, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _tenantContext = tenantContext;
        _env = env;
    }

    /// <summary>List all documents for the current tenant.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TenantDocumentListDto>>>> GetDocuments()
    {
        if (!_tenantContext.TenantId.HasValue)
            return BadRequest(ApiResponse<List<TenantDocumentListDto>>.ErrorResponse("Tenant context not found"));
        var result = await _mediator.Send(new GetTenantDocumentsQuery());
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>Export tenant documents list (metadata) as CSV, Excel (.xlsx), or PDF. format=csv|xlsx|pdf.</summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportDocuments([FromQuery] string format = "csv", CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.TenantId.HasValue)
            return BadRequest();
        var fmt = format?.Trim().ToLowerInvariant() ?? "csv";
        if (fmt != "csv" && fmt != "xlsx" && fmt != "pdf")
            return BadRequest("Supported format: csv, xlsx, pdf");
        var result = await _mediator.Send(new GetTenantDocumentsQuery(), cancellationToken);
        if (!result.Success || result.Data == null)
            return NotFound();
        var items = result.Data;
        var stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var profileResult = await _mediator.Send(new GetTenantProfileQuery(), cancellationToken);
        var header = ExportReportHeaderBuilder.Build(profileResult.Data, "Company Documents");

        if (fmt == "xlsx")
        {
            var bytes = ExportHelper.ToExcel("Documents", "Company Documents", items, header);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"TenantDocuments_{stamp}.xlsx");
        }
        if (fmt == "pdf")
        {
            var columnHeaders = new[] { "Type", "Document #", "File Name", "Expiry", "Created" };
            var rows = items.Select(i => new[]
            {
                i.DocumentType,
                i.DocumentNumber ?? "",
                i.OriginalFileName ?? i.FileName,
                i.ExpiryDate?.ToString("yyyy-MM-dd") ?? "",
                i.CreatedDate.ToString("yyyy-MM-dd HH:mm")
            }).ToList();
            var bytes = ExportHelper.ToPdf("Company Documents", columnHeaders, rows, header);
            return File(bytes, "application/pdf", $"TenantDocuments_{stamp}.pdf");
        }
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };
        using var memory = new MemoryStream();
        using (var writer = new StreamWriter(memory, Encoding.UTF8, leaveOpen: true))
        using (var csv = new CsvWriter(writer, csvConfig))
        {
            await csv.WriteRecordsAsync(items, cancellationToken);
        }
        memory.Position = 0;
        return File(memory.ToArray(), "text/csv", $"TenantDocuments_{stamp}.csv");
    }

    /// <summary>Upload a company document. Use multipart/form-data: documentType, documentNumber (optional), expiryDate (optional), file.</summary>
    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<Guid>>> UploadDocument(
        [FromForm] string documentType,
        [FromForm] string? documentNumber,
        [FromForm] DateTime? expiryDate,
        [FromForm] IFormFile? file)
    {
        if (!_tenantContext.TenantId.HasValue)
            return BadRequest(ApiResponse<Guid>.ErrorResponse("Tenant context not found"));
        if (string.IsNullOrWhiteSpace(documentType))
            return BadRequest(ApiResponse<Guid>.ErrorResponse("DocumentType is required"));
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<Guid>.ErrorResponse("File is required"));

        var docId = Guid.NewGuid();
        var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? ".bin";
        var originalFileName = file.FileName;
        var relativePath = Path.Combine("Documents", "Tenant", _tenantContext.TenantId.Value.ToString(), docId.ToString() + ext).Replace('\\', '/');
        var fullPath = Path.Combine(_env.ContentRootPath, "Documents", "Tenant", _tenantContext.TenantId.Value.ToString(), docId.ToString() + ext);
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(stream);

        var command = new CreateTenantDocumentCommand
        {
            Id = docId,
            DocumentType = documentType.Trim(),
            DocumentNumber = string.IsNullOrWhiteSpace(documentNumber) ? null : documentNumber.Trim(),
            FilePath = relativePath,
            OriginalFileName = originalFileName,
            ExpiryDate = expiryDate
        };
        var result = await _mediator.Send(command);
        if (!result.Success)
        {
            try { System.IO.File.Delete(fullPath); } catch { }
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>Download a document file by id.</summary>
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> DownloadDocument(Guid id)
    {
        var query = new GetTenantDocumentByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        if (!result.Success || result.Data == null)
            return NotFound();
        var fullPath = Path.Combine(_env.ContentRootPath, result.Data.FilePath.Replace('/', Path.DirectorySeparatorChar));
        if (!System.IO.File.Exists(fullPath))
            return NotFound();
        var contentType = GetContentType(Path.GetExtension(fullPath));
        var fileName = result.Data.OriginalFileName ?? Path.GetFileName(result.Data.FilePath) ?? "document";
        return PhysicalFile(fullPath, contentType, fileName, enableRangeProcessing: true);
    }

    /// <summary>Delete a tenant document.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDocument(Guid id)
    {
        var getQuery = new GetTenantDocumentByIdQuery { Id = id };
        var getResult = await _mediator.Send(getQuery);
        if (!getResult.Success || getResult.Data == null)
            return NotFound();

        var result = await _mediator.Send(new DeleteTenantDocumentCommand { Id = id });
        if (!result.Success)
            return BadRequest(result);

        var fullPath = Path.Combine(_env.ContentRootPath, getResult.Data.FilePath.Replace('/', Path.DirectorySeparatorChar));
        try { if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath); } catch { }
        return Ok(result);
    }

    private static string GetContentType(string ext)
    {
        return ext?.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }
}
