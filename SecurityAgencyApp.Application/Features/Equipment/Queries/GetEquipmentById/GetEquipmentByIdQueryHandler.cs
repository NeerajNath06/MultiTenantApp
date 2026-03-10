using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Equipment.Queries.GetEquipmentById;

public class GetEquipmentByIdQueryHandler : IRequestHandler<GetEquipmentByIdQuery, ApiResponse<EquipmentDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetEquipmentByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<EquipmentDetailDto>> Handle(GetEquipmentByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<EquipmentDetailDto>.ErrorResponse("Tenant context not found");

        var equipment = await _unitOfWork.Repository<Domain.Entities.Equipment>().GetByIdAsync(request.Id, cancellationToken);
        if (equipment == null || equipment.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<EquipmentDetailDto>.ErrorResponse("Equipment not found");

        string? guardName = null;
        string? siteName = null;
        if (equipment.AssignedToGuardId.HasValue)
        {
            var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(equipment.AssignedToGuardId.Value, cancellationToken);
            guardName = guard != null ? $"{guard.FirstName} {guard.LastName}".Trim() : null;
        }
        if (equipment.AssignedToSiteId.HasValue)
        {
            var site = await _unitOfWork.Repository<Site>().GetByIdAsync(equipment.AssignedToSiteId.Value, cancellationToken);
            siteName = site?.SiteName;
        }

        var dto = new EquipmentDetailDto
        {
            Id = equipment.Id,
            EquipmentCode = equipment.EquipmentCode,
            EquipmentName = equipment.EquipmentName,
            Category = equipment.Category,
            Manufacturer = equipment.Manufacturer,
            ModelNumber = equipment.ModelNumber,
            SerialNumber = equipment.SerialNumber,
            PurchaseDate = equipment.PurchaseDate,
            PurchaseCost = equipment.PurchaseCost,
            Status = equipment.Status,
            AssignedToGuardId = equipment.AssignedToGuardId,
            AssignedToGuardName = guardName,
            AssignedToSiteId = equipment.AssignedToSiteId,
            AssignedToSiteName = siteName,
            NextMaintenanceDate = equipment.NextMaintenanceDate,
            Notes = equipment.Notes,
            IsActive = equipment.IsActive,
            CreatedDate = equipment.CreatedDate
        };
        return ApiResponse<EquipmentDetailDto>.SuccessResponse(dto, "Equipment retrieved");
    }
}
