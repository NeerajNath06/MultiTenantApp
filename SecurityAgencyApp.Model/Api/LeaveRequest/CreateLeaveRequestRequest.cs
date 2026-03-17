namespace SecurityAgencyApp.Model.Api;

public class CreateLeaveRequestRequest
{
    public Guid GuardId { get; set; }
    public string LeaveType { get; set; } = "Casual";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
