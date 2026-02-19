using System.IO;
using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.TenantDocuments.Queries.GetTenantDocuments;

public class GetTenantDocumentsQueryHandler : IRequestHandler<GetTenantDocumentsQuery, ApiResponse<List<TenantDocumentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetTenantDocumentsQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<List<TenantDocumentDto>>> Handle(GetTenantDocumentsQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<List<TenantDocumentDto>>.ErrorResponse("Tenant context not found");

        var docs = await _unitOfWork.Repository<TenantDocument>()
            .FindAsync(d => d.TenantId == _tenantContext.TenantId.Value, cancellationToken);

        var list = docs.OrderByDescending(d => d.CreatedDate).Select(d => new TenantDocumentDto
        {
            Id = d.Id,
            DocumentType = d.DocumentType,
            DocumentNumber = d.DocumentNumber,
            FileName = string.IsNullOrEmpty(d.FilePath) ? "document" : Path.GetFileName(d.FilePath),
            OriginalFileName = d.OriginalFileName,
            ExpiryDate = d.ExpiryDate,
            CreatedDate = d.CreatedDate
        }).ToList();

        return ApiResponse<List<TenantDocumentDto>>.SuccessResponse(list, "Documents retrieved successfully");
    }
}
