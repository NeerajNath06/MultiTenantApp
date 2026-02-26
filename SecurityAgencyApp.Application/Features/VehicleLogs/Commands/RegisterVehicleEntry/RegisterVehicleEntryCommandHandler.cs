using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.VehicleLogs.Commands.RegisterVehicleEntry;

public class RegisterVehicleEntryCommandHandler : IRequestHandler<RegisterVehicleEntryCommand, ApiResponse<RegisterVehicleEntryResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public RegisterVehicleEntryCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<RegisterVehicleEntryResultDto>> Handle(RegisterVehicleEntryCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<RegisterVehicleEntryResultDto>.ErrorResponse("Tenant context not found");

        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(request.SiteId, cancellationToken);
        if (site == null || site.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<RegisterVehicleEntryResultDto>.ErrorResponse("Invalid site");

        var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(request.GuardId, cancellationToken);
        if (guard == null || guard.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<RegisterVehicleEntryResultDto>.ErrorResponse("Invalid guard");

        var entryTime = DateTime.UtcNow;
        var entity = new VehicleLog
        {
            TenantId = _tenantContext.TenantId.Value,
            VehicleNumber = request.VehicleNumber.Trim().ToUpperInvariant(),
            VehicleType = string.IsNullOrWhiteSpace(request.VehicleType) ? "Car" : request.VehicleType.Trim(),
            DriverName = request.DriverName.Trim(),
            DriverPhone = request.DriverPhone?.Trim(),
            Purpose = string.IsNullOrWhiteSpace(request.Purpose) ? "Visitor" : request.Purpose.Trim(),
            ParkingSlot = request.ParkingSlot?.Trim(),
            SiteId = request.SiteId,
            GuardId = request.GuardId,
            EntryTime = entryTime,
        };

        await _unitOfWork.Repository<VehicleLog>().AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new RegisterVehicleEntryResultDto { Id = entity.Id, EntryTime = entryTime };
        return ApiResponse<RegisterVehicleEntryResultDto>.SuccessResponse(result, "Vehicle entry registered");
    }
}
