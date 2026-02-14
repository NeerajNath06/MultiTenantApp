using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Wages.Queries.GetWageDetailsByGuard;

public class GetWageDetailsByGuardQuery : IRequest<ApiResponse<GuardPayslipsResponseDto>>
{
    public Guid GuardId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 24;
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
}

public class GuardPayslipsResponseDto
{
    public List<GuardPayslipDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class GuardPayslipDto
{
    public Guid Id { get; set; }
    public Guid WageId { get; set; }
    public string WageSheetNumber { get; set; } = string.Empty;
    public DateTime WagePeriodStart { get; set; }
    public DateTime WagePeriodEnd { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DaysWorked { get; set; }
    public int HoursWorked { get; set; }
    public decimal BasicAmount { get; set; }
    public decimal OvertimeAmount { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string? Remarks { get; set; }
}
