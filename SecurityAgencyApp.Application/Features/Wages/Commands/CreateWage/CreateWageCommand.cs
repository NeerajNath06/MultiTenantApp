using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Wages.Commands.CreateWage;

public class CreateWageCommand : IRequest<ApiResponse<Guid>>
{
    public string WageSheetNumber { get; set; } = string.Empty;
    public DateTime WagePeriodStart { get; set; }
    public DateTime WagePeriodEnd { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = "Draft";
    public decimal TotalAllowances { get; set; }
    public decimal TotalDeductions { get; set; }
    public string? Notes { get; set; }
    public List<WageDetailDto> WageDetails { get; set; } = new();
}

public class WageDetailDto
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
