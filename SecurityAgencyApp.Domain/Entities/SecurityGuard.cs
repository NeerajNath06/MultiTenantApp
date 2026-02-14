using SecurityAgencyApp.Domain.Common;
using SecurityAgencyApp.Domain.Enums;

namespace SecurityAgencyApp.Domain.Entities;

public class SecurityGuard : TenantEntity
{
    public string GuardCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AlternatePhone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public string? AadharNumber { get; set; }
    public string? PANNumber { get; set; }
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public DateTime JoiningDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? PhotoPath { get; set; }
    /// <summary>When set, this guard can login to the mobile app with this User's credentials.</summary>
    public Guid? UserId { get; set; }
    /// <summary>User (Supervisor) responsible for this guard. Used for supervisor-based filtering and reporting.</summary>
    public Guid? SupervisorId { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual User? User { get; set; }
    public virtual User? Supervisor { get; set; }
    public virtual ICollection<GuardDocument> Documents { get; set; } = new List<GuardDocument>();
    public virtual ICollection<GuardAssignment> Assignments { get; set; } = new List<GuardAssignment>();
    public virtual ICollection<GuardAttendance> Attendances { get; set; } = new List<GuardAttendance>();
    public virtual ICollection<IncidentReport> IncidentReports { get; set; } = new List<IncidentReport>();
    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public virtual ICollection<TrainingRecord> TrainingRecords { get; set; } = new List<TrainingRecord>();
    public virtual ICollection<WageDetail> WageDetails { get; set; } = new List<WageDetail>();
    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public virtual ICollection<Equipment> AssignedEquipment { get; set; } = new List<Equipment>();
}
