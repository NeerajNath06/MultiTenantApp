namespace SecurityAgencyApp.Model.Api;

public class CreateAssignmentRequest
{
    public Guid GuardId { get; set; }
    public Guid SiteId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid? SupervisorId { get; set; }
    public DateTime AssignmentStartDate { get; set; }
    public DateTime? AssignmentEndDate { get; set; }
    public string? Remarks { get; set; }
}
