using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Attendance.Queries.GetGuardAttendanceByDate;

public class GetGuardAttendanceByDateQuery : IRequest<ApiResponse<List<GuardAttendanceItemDto>>>
{
    public Guid GuardId { get; set; }
    public DateTime? Date { get; set; }
}

public class GuardAttendanceItemDto
{
    public Guid Id { get; set; }
    public Guid GuardId { get; set; }
    public Guid AssignmentId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public double? SiteLatitude { get; set; }
    public double? SiteLongitude { get; set; }
    public int? SiteGeofenceRadiusMeters { get; set; }
    public DateTime AttendanceDate { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string? Remarks { get; set; }
}
