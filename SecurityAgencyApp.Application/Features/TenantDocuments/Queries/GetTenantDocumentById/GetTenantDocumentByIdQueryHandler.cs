using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.TenantDocuments.Queries.GetTenantDocumentById;

public class GetTenantDocumentByIdQueryHandler : IRequestHandler<GetTenantDocumentByIdQuery, ApiResponse<TenantDocumentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetTenantDocumentByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<TenantDocumentDto>> Handle(GetTenantDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<TenantDocumentDto>.ErrorResponse("Tenant context not found");

        var doc = await _unitOfWork.Repository<TenantDocument>().GetByIdAsync(request.Id, cancellationToken);
        if (doc == null || doc.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<TenantDocumentDto>.ErrorResponse("Document not found");

        return ApiResponse<TenantDocumentDto>.SuccessResponse(new TenantDocumentDto
        {
            Id = doc.Id,
            TenantId = doc.TenantId,
            FilePath = doc.FilePath,
            DocumentType = doc.DocumentType,
            OriginalFileName = doc.OriginalFileName
        }, "Document retrieved successfully");
    }
}
