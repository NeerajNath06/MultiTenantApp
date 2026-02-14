using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class LeaveRequest : TenantEntity
{
    public Guid GuardId { get; set; }
    public string LeaveType { get; set; } = "Casual"; // Casual, Sick, Emergency, Annual, Unpaid
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Cancelled
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual SecurityGuard Guard { get; set; } = null!;
    public virtual User? ApprovedByUser { get; set; }
}
