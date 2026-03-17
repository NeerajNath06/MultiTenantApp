namespace SecurityAgencyApp.Model.Api;

public class MarkAttendanceRequest
{
    public Guid GuardId { get; set; }
    public Guid AssignmentId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public string? CheckInLocation { get; set; }
    public string? CheckOutLocation { get; set; }
    public string Status { get; set; } = "Present"; // Present, Absent, Leave, HalfDay
    public string? Remarks { get; set; }
}
