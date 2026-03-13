using System.ComponentModel.DataAnnotations;

namespace SecurityAgencyApp.Web.Models.Api;

public class UserListResponse
{
    public List<UserItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UserItemDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? DesignationId { get; set; }
    public string? DesignationName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginDate { get; set; }
}

public class UserDetailDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AadharNumber { get; set; }
    public string? PANNumber { get; set; }
    public string? UAN { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? DesignationId { get; set; }
    public string? DesignationName { get; set; }
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<Guid> RoleIds { get; set; } = new();
    public DateTime CreatedDate { get; set; }
}

public class CreateUserRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? LastName { get; set; }

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
    /// <summary>Role IDs to assign (e.g. Supervisor role to create a supervisor user).</summary>
    public List<Guid>? RoleIds { get; set; }
}

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
