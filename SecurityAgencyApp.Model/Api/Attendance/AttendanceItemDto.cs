namespace SecurityAgencyApp.Model.Api;

public class AttendanceItemDto
{
    public Guid Id { get; set; }
    public Guid GuardId { get; set; }
    public string GuardName { get; set; } = string.Empty;
    public string GuardCode { get; set; } = string.Empty;
    public string? GuardPhone { get; set; }
    public Guid AssignmentId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}
