using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.TenantDocuments.Commands.CreateTenantDocument;

public class CreateTenantDocumentCommandHandler : IRequestHandler<CreateTenantDocumentCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateTenantDocumentCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateTenantDocumentCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");

        var tenant = await _unitOfWork.Repository<Tenant>().GetByIdAsync(_tenantContext.TenantId.Value, cancellationToken);
        if (tenant == null)
            return ApiResponse<Guid>.ErrorResponse("Tenant not found");

        var doc = new TenantDocument
        {
            TenantId = _tenantContext.TenantId.Value,
            DocumentType = request.DocumentType.Trim(),
            DocumentNumber = string.IsNullOrWhiteSpace(request.DocumentNumber) ? null : request.DocumentNumber.Trim(),
            FilePath = request.FilePath,
            OriginalFileName = string.IsNullOrWhiteSpace(request.OriginalFileName) ? null : request.OriginalFileName.Trim(),
            ExpiryDate = request.ExpiryDate
        };
        if (request.Id.HasValue)
            doc.Id = request.Id.Value;

        await _unitOfWork.Repository<TenantDocument>().AddAsync(doc, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<Guid>.SuccessResponse(doc.Id, "Document uploaded successfully");
    }
}
