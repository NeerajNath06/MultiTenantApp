using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.GuardDocuments.Queries.GetGuardDocuments;

public class GetGuardDocumentsQuery : IRequest<ApiResponse<List<GuardDocumentDto>>>
{
    public Guid GuardId { get; set; }
}

public class GuardDocumentDto
{
    public Guid Id { get; set; }
    public Guid GuardId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedDate { get; set; }
}
