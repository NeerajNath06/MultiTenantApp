using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Attendance.Commands.CheckOut;

public class CheckOutCommand : IRequest<ApiResponse<CheckOutResultDto>>
{
    public Guid AttendanceId { get; set; }
    public Guid GuardId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    /// <summary>Client (mobile) current time in ISO format; when provided, used as check-out time.</summary>
    public DateTime? CheckOutTime { get; set; }
    public string? PhotoBase64 { get; set; }
    public string? PhotoFileName { get; set; }
    public string? Notes { get; set; }
}

public class CheckOutResultDto
{
    public string Message { get; set; } = "Check-out successful.";
}
