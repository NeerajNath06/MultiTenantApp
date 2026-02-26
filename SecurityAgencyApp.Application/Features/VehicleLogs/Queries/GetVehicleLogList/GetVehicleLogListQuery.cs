using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.VehicleLogs.Queries.GetVehicleLogList;

public class GetVehicleLogListQuery : IRequest<ApiResponse<VehicleLogListResponseDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? Search { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? GuardId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    /// <summary>null = all, true = only inside (no ExitTime), false = only exited</summary>
    public bool? InsideOnly { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "desc";
}

public class VehicleLogListResponseDto
{
    public List<VehicleLogDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class VehicleLogDto
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
