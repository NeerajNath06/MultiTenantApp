using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.TenantDocuments.Queries.GetTenantDocuments;

public class GetTenantDocumentsQuery : IRequest<ApiResponse<List<TenantDocumentDto>>>
{
}

public class TenantDocumentDto
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? OriginalFileName { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedDate { get; set; }
}
