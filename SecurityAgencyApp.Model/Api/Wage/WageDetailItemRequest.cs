namespace SecurityAgencyApp.Model.Api;

public class WageDetailItemRequest
{
    public Guid GuardId { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? ShiftId { get; set; }
    public int DaysWorked { get; set; }
    public int HoursWorked { get; set; }
    public decimal BasicRate { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal OvertimeRate { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public string? Remarks { get; set; }
}
