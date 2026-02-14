using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class User : TenantEntity
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    /// <summary>Plain password for recovery lookup only (e.g. forgot password / admin support). Do not expose in APIs by default.</summary>
    public string? Password { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? DesignationId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;
    public DateTime? LastLoginDate { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Department? Department { get; set; }
    public virtual Designation? Designation { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    public virtual ICollection<GuardAssignment> CreatedAssignments { get; set; } = new List<GuardAssignment>();
    public virtual ICollection<GuardAssignment> SupervisedAssignments { get; set; } = new List<GuardAssignment>();
    /// <summary>Guards assigned to this user when they act as Supervisor.</summary>
    public virtual ICollection<SecurityGuard> SupervisedGuards { get; set; } = new List<SecurityGuard>();
    public virtual ICollection<UserMenu> UserMenus { get; set; } = new List<UserMenu>();
    public virtual ICollection<UserSubMenu> UserSubMenus { get; set; } = new List<UserSubMenu>();
    public virtual ICollection<FormTemplate> CreatedFormTemplates { get; set; } = new List<FormTemplate>();
    public virtual ICollection<FormSubmission> FormSubmissions { get; set; } = new List<FormSubmission>();
    public virtual ICollection<GuardDocument> VerifiedDocuments { get; set; } = new List<GuardDocument>();
    /// <summary>Sites this user supervises (when logged in as supervisor, sees only these sites and guards on them).</summary>
    public virtual ICollection<SiteSupervisor> SupervisedSites { get; set; } = new List<SiteSupervisor>();
}
