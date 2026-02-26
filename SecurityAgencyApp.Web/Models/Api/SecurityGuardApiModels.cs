namespace SecurityAgencyApp.Web.Models.Api;

public class GuardListResponse
{
    public List<GuardItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public string? Search { get; set; }
}

public class GuardItemDto
{
    public Guid Id { get; set; }
    public string GuardCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid? SupervisorId { get; set; }
    public string? SupervisorName { get; set; }
}

public class GuardDetailDto
{
    public Guid Id { get; set; }
    public string GuardCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PinCode { get; set; }
    public string? AadharNumber { get; set; }
    public string? PANNumber { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public DateTime? JoiningDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public Guid? SupervisorId { get; set; }
    public string? SupervisorName { get; set; }
}

public class CreateGuardRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AlternatePhone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty; // "Male", "Female", "Other"
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public string? AadharNumber { get; set; }
    public string? PANNumber { get; set; }
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public DateTime JoiningDate { get; set; }
    public bool CreateLoginAccount { get; set; }
    public string? LoginUserName { get; set; }
    public string? LoginPassword { get; set; }
    public Guid? SupervisorId { get; set; }
}

public class UpdateGuardRequest
{
    public Guid Id { get; set; }
    public string GuardCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public string? AadharNumber { get; set; }
    public string? PANNumber { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public DateTime? JoiningDate { get; set; }
    public bool IsActive { get; set; }
    public Guid? SupervisorId { get; set; }
}
