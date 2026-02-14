using SecurityAgencyApp.Domain.Common;
using SecurityAgencyApp.Domain.Enums;

namespace SecurityAgencyApp.Domain.Entities;

public class GuardAssignment : TenantEntity
{
    public Guid GuardId { get; set; }
    public Guid SiteId { get; set; }
    public Guid? SupervisorId { get; set; }
    public Guid ShiftId { get; set; }
    public DateTime AssignmentStartDate { get; set; }
    public DateTime? AssignmentEndDate { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Active;
    public string? Remarks { get; set; }
    public Guid CreatedBy { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual SecurityGuard Guard { get; set; } = null!;
    public virtual Site Site { get; set; } = null!;
    public virtual User? Supervisor { get; set; }
    public virtual Shift Shift { get; set; } = null!;
    public virtual User CreatedByUser { get; set; } = null!;
    public virtual ICollection<GuardAttendance> Attendances { get; set; } = new List<GuardAttendance>();
}
