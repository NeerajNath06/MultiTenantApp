using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.GuardDocuments.Queries.GetGuardDocumentById;

public class GetGuardDocumentByIdQuery : IRequest<ApiResponse<GuardDocumentDto>>
{
    public Guid Id { get; set; }
}

public class GuardDocumentDto
{
    public Guid Id { get; set; }
    public Guid GuardId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
}
