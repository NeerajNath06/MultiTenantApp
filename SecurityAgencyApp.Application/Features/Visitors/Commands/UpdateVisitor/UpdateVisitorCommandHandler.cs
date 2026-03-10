using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Visitors.Commands.UpdateVisitor;

public class UpdateVisitorCommandHandler : IRequestHandler<UpdateVisitorCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateVisitorCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateVisitorCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");

        var repo = _unitOfWork.Repository<Visitor>();
        var entity = await repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null || entity.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<bool>.ErrorResponse("Visitor not found");

        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(request.SiteId, cancellationToken);
        if (site == null || site.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<bool>.ErrorResponse("Invalid site");
        var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(request.GuardId, cancellationToken);
        if (guard == null || guard.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<bool>.ErrorResponse("Invalid guard");

        entity.VisitorName = request.VisitorName.Trim();
        entity.VisitorType = string.IsNullOrWhiteSpace(request.VisitorType) ? "Individual" : request.VisitorType.Trim();
        entity.CompanyName = request.CompanyName?.Trim();
        entity.PhoneNumber = request.PhoneNumber.Trim();
        entity.Email = request.Email?.Trim();
        entity.Purpose = request.Purpose.Trim();
        entity.HostName = request.HostName?.Trim();
        entity.HostDepartment = request.HostDepartment?.Trim();
        entity.SiteId = request.SiteId;
        entity.GuardId = request.GuardId;
        entity.IdProofType = request.IdProofType?.Trim();
        entity.IdProofNumber = request.IdProofNumber?.Trim();
        entity.ModifiedDate = DateTime.UtcNow;

        await repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Visitor updated successfully");
    }
}
