using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.TenantDocuments.Queries.GetTenantDocumentById;

public class GetTenantDocumentByIdQuery : IRequest<ApiResponse<TenantDocumentDto>>
{
    public Guid Id { get; set; }
}

public class TenantDocumentDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string? OriginalFileName { get; set; }
}
