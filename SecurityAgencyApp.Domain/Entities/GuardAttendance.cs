using SecurityAgencyApp.Domain.Common;
using SecurityAgencyApp.Domain.Enums;

namespace SecurityAgencyApp.Domain.Entities;

public class GuardAttendance : TenantEntity
{
    public Guid GuardId { get; set; }
    public Guid AssignmentId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string? CheckInLocation { get; set; }
    public string? CheckOutLocation { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Absent;
    public string? Remarks { get; set; }
    public Guid? MarkedBy { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual SecurityGuard Guard { get; set; } = null!;
    public virtual GuardAssignment Assignment { get; set; } = null!;
    public virtual User? MarkedByUser { get; set; }
}
