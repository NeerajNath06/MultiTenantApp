using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.GuardDocuments.Commands.CreateGuardDocument;
using SecurityAgencyApp.Application.Features.GuardDocuments.Queries.GetGuardDocumentById;
using SecurityAgencyApp.Application.Features.GuardDocuments.Queries.GetGuardDocuments;
using SecurityAgencyApp.Application.Interfaces;
using GuardDocumentListDto = SecurityAgencyApp.Application.Features.GuardDocuments.Queries.GetGuardDocuments.GuardDocumentDto;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class GuardDocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantContext _tenantContext;
    private readonly IWebHostEnvironment _env;

    public GuardDocumentsController(IMediator mediator, ITenantContext tenantContext, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _tenantContext = tenantContext;
        _env = env;
    }

    /// <summary>List documents for a guard.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<GuardDocumentListDto>>>> GetDocuments([FromQuery] Guid guardId)
    {
        if (guardId == Guid.Empty)
            return BadRequest(ApiResponse<List<GuardDocumentListDto>>.ErrorResponse("GuardId is required"));
        var query = new GetGuardDocumentsQuery { GuardId = guardId };
        var result = await _mediator.Send(query);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>Upload a document for a guard. Use multipart/form-data: guardId, documentType, documentNumber (optional), expiryDate (optional), file.</summary>
    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<Guid>>> UploadDocument(
        [FromForm] Guid guardId,
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
        var relativePath = Path.Combine("Documents", _tenantContext.TenantId.Value.ToString(), guardId.ToString(), docId.ToString() + ext).Replace('\\', '/');
        var fullPath = Path.Combine(_env.ContentRootPath, "Documents", _tenantContext.TenantId.Value.ToString(), guardId.ToString(), docId.ToString() + ext);
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(stream);

        var command = new CreateGuardDocumentCommand
        {
            Id = docId,
            GuardId = guardId,
            DocumentType = documentType.Trim(),
            DocumentNumber = string.IsNullOrWhiteSpace(documentNumber) ? null : documentNumber.Trim(),
            FilePath = relativePath,
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
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadDocument(Guid id)
    {
        var query = new GetGuardDocumentByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        if (!result.Success || result.Data == null)
            return NotFound();
        var fullPath = Path.Combine(_env.ContentRootPath, result.Data.FilePath.Replace('/', Path.DirectorySeparatorChar));
        if (!System.IO.File.Exists(fullPath))
            return NotFound();
        var contentType = GetContentType(Path.GetExtension(fullPath));
        var fileName = Path.GetFileName(result.Data.FilePath) ?? "document";
        return PhysicalFile(fullPath, contentType, fileName, enableRangeProcessing: true);
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
