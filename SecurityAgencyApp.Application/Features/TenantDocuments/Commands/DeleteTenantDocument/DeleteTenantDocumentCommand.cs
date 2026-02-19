using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.TenantDocuments.Commands.DeleteTenantDocument;

public class DeleteTenantDocumentCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
