using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.VehicleLogs.Commands.RecordVehicleExit;

public class RecordVehicleExitCommandHandler : IRequestHandler<RecordVehicleExitCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public RecordVehicleExitCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(RecordVehicleExitCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");

        var repo = _unitOfWork.Repository<VehicleLog>();
        var entity = await repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null || entity.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<bool>.ErrorResponse("Vehicle log not found");

        if (entity.ExitTime.HasValue)
            return ApiResponse<bool>.ErrorResponse("Vehicle exit already recorded");

        entity.ExitTime = request.ExitTime;
        entity.ModifiedDate = DateTime.UtcNow;
        await repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Vehicle exit recorded");
    }
}
