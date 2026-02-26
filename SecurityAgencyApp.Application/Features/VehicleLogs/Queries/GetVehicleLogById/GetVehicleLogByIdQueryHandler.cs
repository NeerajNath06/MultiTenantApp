using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.VehicleLogs.Queries.GetVehicleLogById;

public class GetVehicleLogByIdQueryHandler : IRequestHandler<GetVehicleLogByIdQuery, ApiResponse<VehicleLogDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetVehicleLogByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<VehicleLogDto>> Handle(GetVehicleLogByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<VehicleLogDto>.ErrorResponse("Tenant context not found");

        var entity = await _unitOfWork.Repository<VehicleLog>().GetByIdAsync(request.Id, cancellationToken);
        if (entity == null || entity.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<VehicleLogDto>.ErrorResponse("Vehicle log not found");

        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(entity.SiteId, cancellationToken);
        var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(entity.GuardId, cancellationToken);

        var dto = new VehicleLogDto
        {
            Id = entity.Id,
            VehicleNumber = entity.VehicleNumber,
            VehicleType = entity.VehicleType,
            DriverName = entity.DriverName,
            DriverPhone = entity.DriverPhone,
            Purpose = entity.Purpose,
            ParkingSlot = entity.ParkingSlot,
            SiteId = entity.SiteId,
            SiteName = site?.SiteName,
            GuardId = entity.GuardId,
            GuardName = guard != null ? $"{guard.FirstName} {guard.LastName}".Trim() : null,
            EntryTime = entity.EntryTime,
            ExitTime = entity.ExitTime,
        };
        return ApiResponse<VehicleLogDto>.SuccessResponse(dto, "Vehicle log retrieved");
    }
}
