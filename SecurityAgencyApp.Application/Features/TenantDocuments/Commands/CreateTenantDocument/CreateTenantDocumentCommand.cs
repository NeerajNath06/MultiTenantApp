using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.TenantDocuments.Commands.CreateTenantDocument;

public class CreateTenantDocumentCommand : IRequest<ApiResponse<Guid>>
{
    public Guid? Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? OriginalFileName { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
