using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Wages.Queries.GetWageWithDetails;

public class GetWageWithDetailsQuery : IRequest<ApiResponse<WageWithDetailsDto>>
{
    public Guid WageId { get; set; }
    /// <summary>Optional: return only details for this site.</summary>
    public Guid? SiteId { get; set; }
}

public class WageWithDetailsDto
{
    public Guid Id { get; set; }
    public string WageSheetNumber { get; set; } = string.Empty;
    public DateTime WagePeriodStart { get; set; }
    public DateTime WagePeriodEnd { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalWages { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetAmount { get; set; }
    public string? Notes { get; set; }
    public List<WageDetailRowDto> Details { get; set; } = new();
}

public class WageDetailRowDto
{
    public Guid Id { get; set; }
    public Guid GuardId { get; set; }
    public string GuardName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? UAN { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? IFSCCode { get; set; }
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
