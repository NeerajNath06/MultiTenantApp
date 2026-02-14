using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.GuardDocuments.Queries.GetGuardDocuments;

public class GetGuardDocumentsQueryHandler : IRequestHandler<GetGuardDocumentsQuery, ApiResponse<List<GuardDocumentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetGuardDocumentsQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<List<GuardDocumentDto>>> Handle(GetGuardDocumentsQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<List<GuardDocumentDto>>.ErrorResponse("Tenant context not found");

        var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(request.GuardId, cancellationToken);
        if (guard == null || guard.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<List<GuardDocumentDto>>.ErrorResponse("Guard not found");

        var docs = await _unitOfWork.Repository<GuardDocument>()
            .FindAsync(d => d.GuardId == request.GuardId, cancellationToken);

        var list = docs.OrderByDescending(d => d.CreatedDate).Select(d => new GuardDocumentDto
        {
            Id = d.Id,
            GuardId = d.GuardId,
            DocumentType = d.DocumentType,
            DocumentNumber = d.DocumentNumber,
            FileName = string.IsNullOrEmpty(d.FilePath) ? "document" : System.IO.Path.GetFileName(d.FilePath),
            ExpiryDate = d.ExpiryDate,
            IsVerified = d.IsVerified,
            CreatedDate = d.CreatedDate
        }).ToList();

        return ApiResponse<List<GuardDocumentDto>>.SuccessResponse(list, "Documents retrieved successfully");
    }
}
