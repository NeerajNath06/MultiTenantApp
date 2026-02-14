using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.GuardDocuments.Commands.CreateGuardDocument;

public class CreateGuardDocumentCommand : IRequest<ApiResponse<Guid>>
{
    /// <summary>When set, the new document entity will use this Id (e.g. for file name consistency).</summary>
    public Guid? Id { get; set; }
    public Guid GuardId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
}
