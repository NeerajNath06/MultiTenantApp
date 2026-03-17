using System.ComponentModel.DataAnnotations;

namespace SecurityAgencyApp.Model.Api;

public class UpdateUserRequest
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? LastName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [RegularExpression("^[0-9]{12}$", ErrorMessage = "Aadhar number must be exactly 12 digits")]
    public string? AadharNumber { get; set; }

    [RegularExpression("^[A-Za-z]{5}[0-9]{4}[A-Za-z]{1}$", ErrorMessage = "PAN number must be in format AAAAA9999A")]
    public string? PANNumber { get; set; }

    [StringLength(50)]
    public string? UAN { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? DesignationId { get; set; }
    public bool IsActive { get; set; }
    public List<Guid>? RoleIds { get; set; }
}
