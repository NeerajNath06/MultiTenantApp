using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Equipment.Queries.GetEquipmentById;

public class GetEquipmentByIdQuery : IRequest<ApiResponse<EquipmentDetailDto>>
{
    public Guid Id { get; set; }
}

public class EquipmentDetailDto
{
    public Guid Id { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? ModelNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? AssignedToGuardId { get; set; }
    public string? AssignedToGuardName { get; set; }
    public Guid? AssignedToSiteId { get; set; }
    public string? AssignedToSiteName { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
