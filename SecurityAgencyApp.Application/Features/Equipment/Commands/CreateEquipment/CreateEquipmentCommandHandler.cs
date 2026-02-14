using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Equipment.Commands.CreateEquipment;

public class CreateEquipmentCommandHandler : IRequestHandler<CreateEquipmentCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateEquipmentCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateEquipmentCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        // Check if equipment code exists
        var equipmentRepo = _unitOfWork.Repository<Domain.Entities.Equipment>();
        var existing = await equipmentRepo.FirstOrDefaultAsync(
            e => e.EquipmentCode == request.EquipmentCode && e.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        if (existing != null)
        {
            return ApiResponse<Guid>.ErrorResponse("Equipment code already exists");
        }

        // Validate guard if assigned
        if (request.AssignedToGuardId.HasValue)
        {
            var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(request.AssignedToGuardId.Value, cancellationToken);
            if (guard == null || guard.TenantId != _tenantContext.TenantId.Value)
            {
                return ApiResponse<Guid>.ErrorResponse("Invalid security guard");
            }
        }

        // Validate site if assigned
        if (request.AssignedToSiteId.HasValue)
        {
            var site = await _unitOfWork.Repository<Site>().GetByIdAsync(request.AssignedToSiteId.Value, cancellationToken);
            if (site == null || site.TenantId != _tenantContext.TenantId.Value)
            {
                return ApiResponse<Guid>.ErrorResponse("Invalid site");
            }
        }

        var equipment = new Domain.Entities.Equipment
        {
            TenantId = _tenantContext.TenantId.Value,
            EquipmentCode = request.EquipmentCode,
            EquipmentName = request.EquipmentName,
            Category = request.Category,
            Manufacturer = request.Manufacturer,
            ModelNumber = request.ModelNumber,
            SerialNumber = request.SerialNumber,
            PurchaseDate = request.PurchaseDate,
            PurchaseCost = request.PurchaseCost,
            Status = request.Status,
            AssignedToGuardId = request.AssignedToGuardId,
            AssignedToSiteId = request.AssignedToSiteId,
            AssignedDate = (request.AssignedToGuardId.HasValue || request.AssignedToSiteId.HasValue) ? DateTime.UtcNow : null,
            LastMaintenanceDate = request.LastMaintenanceDate,
            NextMaintenanceDate = request.NextMaintenanceDate,
            Notes = request.Notes,
            IsActive = true
        };

        await equipmentRepo.AddAsync(equipment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(equipment.Id, "Equipment created successfully");
    }
}
