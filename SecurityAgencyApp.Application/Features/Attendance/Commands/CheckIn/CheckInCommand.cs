using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Attendance.Commands.CheckIn;

public class CheckInCommand : IRequest<ApiResponse<CheckInResultDto>>
{
    public Guid GuardId { get; set; }
    public Guid SiteId { get; set; }
    public Guid ShiftId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    /// <summary>Client (mobile) current time in ISO format; when provided, used as check-in time.</summary>
    public DateTime? CheckInTime { get; set; }
    public string? PhotoBase64 { get; set; }
    public string? PhotoFileName { get; set; }
    public string? Notes { get; set; }
}

public class CheckInResultDto
{
    public Guid AttendanceId { get; set; }
    public string Message { get; set; } = "Check-in successful.";
}
