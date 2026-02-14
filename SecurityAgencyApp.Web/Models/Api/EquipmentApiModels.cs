namespace SecurityAgencyApp.Web.Models.Api;

public class EquipmentListResponse
{
    public List<EquipmentItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class EquipmentItemDto
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

public class CreateEquipmentRequest
{
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? ModelNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public string Status { get; set; } = "Available";
    public Guid? AssignedToGuardId { get; set; }
    public Guid? AssignedToSiteId { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? Notes { get; set; }
}
