using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.PatrolScans.Commands.CreatePatrolScan;

public class CreatePatrolScanCommandHandler : IRequestHandler<CreatePatrolScanCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreatePatrolScanCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreatePatrolScanCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");

        var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(request.GuardId, cancellationToken);
        if (guard == null || guard.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<Guid>.ErrorResponse("Invalid guard");

        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(request.SiteId, cancellationToken);
        if (site == null || site.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<Guid>.ErrorResponse("Invalid site");

        var entity = new PatrolScan
        {
            TenantId = _tenantContext.TenantId.Value,
            GuardId = request.GuardId,
            SiteId = request.SiteId,
            ScannedAt = DateTime.UtcNow,
            LocationName = request.LocationName.Trim(),
            CheckpointCode = request.CheckpointCode?.Trim(),
            Status = "Success"
        };

        await _unitOfWork.Repository<PatrolScan>().AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<Guid>.SuccessResponse(entity.Id, "Checkpoint scanned");
    }
}
