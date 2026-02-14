using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.GuardDocuments.Queries.GetGuardDocumentById;

public class GetGuardDocumentByIdQueryHandler : IRequestHandler<GetGuardDocumentByIdQuery, ApiResponse<GuardDocumentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetGuardDocumentByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<GuardDocumentDto>> Handle(GetGuardDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<GuardDocumentDto>.ErrorResponse("Tenant context not found");

        var doc = await _unitOfWork.Repository<GuardDocument>().GetByIdAsync(request.Id, cancellationToken);
        if (doc == null)
            return ApiResponse<GuardDocumentDto>.ErrorResponse("Document not found");

        var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(doc.GuardId, cancellationToken);
        if (guard == null || guard.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<GuardDocumentDto>.ErrorResponse("Document not found");

        return ApiResponse<GuardDocumentDto>.SuccessResponse(new GuardDocumentDto
        {
            Id = doc.Id,
            GuardId = doc.GuardId,
            FilePath = doc.FilePath,
            DocumentType = doc.DocumentType
        }, "Document retrieved successfully");
    }
}
