namespace SecurityAgencyApp.Web.Models.Api;

public class VehicleLogListResponse
{
    public List<VehicleLogItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class VehicleLogItemDto
{
    public Guid Id { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string? DriverPhone { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? ParkingSlot { get; set; }
    public Guid SiteId { get; set; }
    public string? SiteName { get; set; }
    public Guid GuardId { get; set; }
    public string? GuardName { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
}

public class VehicleLogDetailDto
{
    public Guid Id { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string? DriverPhone { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? ParkingSlot { get; set; }
    public Guid SiteId { get; set; }
    public string? SiteName { get; set; }
    public Guid GuardId { get; set; }
    public string? GuardName { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
}

public class CreateVehicleEntryRequest
{
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = "Car";
    public string DriverName { get; set; } = string.Empty;
    public string? DriverPhone { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? ParkingSlot { get; set; }
    public Guid SiteId { get; set; }
    public Guid GuardId { get; set; }
}

public class VehicleLogSummaryBySiteResponse
{
    public List<VehicleLogSiteSummaryDto> Sites { get; set; } = new();
}

public class VehicleLogSiteSummaryDto
{
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string? SiteAddress { get; set; }
    public int TotalEntries { get; set; }
    public int InsideCount { get; set; }
    public int ExitedCount { get; set; }
}
