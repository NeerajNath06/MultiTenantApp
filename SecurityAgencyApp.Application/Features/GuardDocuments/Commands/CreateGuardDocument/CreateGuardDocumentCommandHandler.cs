using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.GuardDocuments.Commands.CreateGuardDocument;

public class CreateGuardDocumentCommandHandler : IRequestHandler<CreateGuardDocumentCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateGuardDocumentCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateGuardDocumentCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");

        var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(request.GuardId, cancellationToken);
        if (guard == null || guard.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<Guid>.ErrorResponse("Guard not found");

        var doc = new GuardDocument
        {
            GuardId = request.GuardId,
            DocumentType = request.DocumentType,
            DocumentNumber = request.DocumentNumber,
            FilePath = request.FilePath,
            ExpiryDate = request.ExpiryDate,
            IsVerified = false
        };
        if (request.Id.HasValue)
            doc.Id = request.Id.Value;
        await _unitOfWork.Repository<GuardDocument>().AddAsync(doc, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<Guid>.SuccessResponse(doc.Id, "Document uploaded successfully");
    }
}
