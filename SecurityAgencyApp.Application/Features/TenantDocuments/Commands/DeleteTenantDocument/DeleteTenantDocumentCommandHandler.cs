using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.TenantDocuments.Commands.DeleteTenantDocument;

public class DeleteTenantDocumentCommandHandler : IRequestHandler<DeleteTenantDocumentCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteTenantDocumentCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteTenantDocumentCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");

        var doc = await _unitOfWork.Repository<TenantDocument>().GetByIdAsync(request.Id, cancellationToken);
        if (doc == null || doc.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<bool>.ErrorResponse("Document not found");

        await _unitOfWork.Repository<TenantDocument>().DeleteAsync(doc, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Document deleted successfully");
    }
}
