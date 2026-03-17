namespace SecurityAgencyApp.Model.Api;

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
