using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Equipment.Queries.GetEquipmentList;

public class GetEquipmentListQuery : IRequest<ApiResponse<EquipmentListResponseDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public Guid? AssignedToGuardId { get; set; }
    public Guid? AssignedToSiteId { get; set; }
    public bool IncludeInactive { get; set; } = false;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc";
}

public class EquipmentListResponseDto
{
    public List<EquipmentDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class EquipmentDto
{
    public Guid Id { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? ModelNumber { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? AssignedToGuardId { get; set; }
    public string? AssignedToGuardName { get; set; }
    public Guid? AssignedToSiteId { get; set; }
    public string? AssignedToSiteName { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
