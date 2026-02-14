using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Domain.Enums;

namespace SecurityAgencyApp.Application.Features.Attendance.Commands.MarkAttendance;

public class MarkAttendanceCommand : IRequest<ApiResponse<Guid>>
{
    public Guid GuardId { get; set; }
    public Guid AssignmentId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string? CheckInLocation { get; set; }
    public string? CheckOutLocation { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }
}
