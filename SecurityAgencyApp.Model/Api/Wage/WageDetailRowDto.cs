namespace SecurityAgencyApp.Model.Api;

public class WageDetailRowDto
{
    public Guid Id { get; set; }
    public Guid GuardId { get; set; }
    public string GuardName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? EmployeeCode { get; set; }
    public int DaysWorked { get; set; }
    public decimal BasicRate { get; set; }
    public decimal BasicAmount { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal OvertimeAmount { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal NetAmount { get; set; }
}
