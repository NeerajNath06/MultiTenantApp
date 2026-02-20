using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.API.Services;
using SecurityAgencyApp.Application.Features.Attendance.Queries.GetAttendanceList;
using SecurityAgencyApp.Application.Features.Bills.Queries.GetBillById;
using SecurityAgencyApp.Application.Features.Bills.Queries.GetBillList;
using SecurityAgencyApp.Application.Features.TenantProfile.Queries.GetTenantProfile;
using SecurityAgencyApp.Application.Features.Wages.Queries.GetWageList;
using SecurityAgencyApp.Application.Features.Wages.Queries.GetWageWithDetails;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Download agency monthly Excel (4 sheets: BILL, ATTENDANCE, Wages, Other) like reference format.</summary>
    [HttpGet("monthly-excel")]
    public async Task<IActionResult> GetMonthlyExcel(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] Guid? siteId = null,
        [FromQuery] Guid? wageId = null,
        [FromQuery] Guid? billId = null,
        CancellationToken cancellationToken = default)
    {
        if (month < 1 || month > 12) return BadRequest("Invalid month");
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var profileResult = await _mediator.Send(new GetTenantProfileQuery(), cancellationToken);
        var header = ExportReportHeaderBuilder.BuildWithDateRange(profileResult.Data, "Monthly Report", startDate, endDate);
        var locationName = siteId.HasValue ? "Site" : "All Sites"; // Could resolve site name via query
        var service = new AgencyMonthlyExcelService(header, locationName, year, month);

        // Attendance: day-wise P/A grid
        var attQuery = new GetAttendanceListQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            PageNumber = 1,
            PageSize = 5000
        };
        var attResult = await _mediator.Send(attQuery, cancellationToken);
        var attItems = attResult.Success && attResult.Data?.Items != null ? attResult.Data.Items : new List<AttendanceDto>();
        var guardDays = attItems
            .GroupBy(a => new { a.GuardId, a.GuardName })
            .Select(g => new
            {
                g.Key.GuardId,
                g.Key.GuardName,
                Days = g.ToDictionary(x => x.AttendanceDate.Day, x => x.Status == "Present" ? "P" : "A")
            })
            .ToList();
        var attendanceRows = guardDays.Select((g, i) => new AgencyMonthlyExcelService.AttendanceDayRow
        {
            SNo = i + 1,
            GuardName = g.GuardName,
            Designation = "S/G",
            DayStatus = g.Days
        }).ToList();

        // Wages: use wageId or first wage for period
        Guid? useWageId = wageId;
        if (!useWageId.HasValue)
        {
            var wageListResult = await _mediator.Send(new GetWageListQuery
            {
                PeriodStart = startDate,
                PeriodEnd = endDate,
                PageNumber = 1,
                PageSize = 1
            }, cancellationToken);
            useWageId = wageListResult.Data?.Items?.FirstOrDefault()?.Id;
        }
        var wageRows = new List<AgencyMonthlyExcelService.WageSheetRow>();
        var otherRows = new List<AgencyMonthlyExcelService.OtherSheetRow>();
        if (useWageId.HasValue)
        {
            var wageDetailResult = await _mediator.Send(new GetWageWithDetailsQuery { WageId = useWageId.Value }, cancellationToken);
            if (wageDetailResult.Success && wageDetailResult.Data?.Details != null)
            {
                var details = wageDetailResult.Data.Details;
                int sn = 1;
                foreach (var d in details)
                {
                    wageRows.Add(new AgencyMonthlyExcelService.WageSheetRow
                    {
                        SNo = sn,
                        Name = d.GuardName,
                        Designation = d.Designation ?? "S/G",
                        UAN = d.UAN,
                        BasicPerDay = d.BasicRate,
                        Attendance = d.DaysWorked,
                        OTHrs = d.OvertimeHours,
                        ED = 0,
                        EarnWages = d.BasicAmount,
                        EpfWagesAt15000 = 0,
                        EmployeeShareEPF12 = 0,
                        EmployeeShareESIC075 = 0,
                        TotalDeduction = d.Deductions,
                        OTPayment = d.OvertimeAmount,
                        EDPayment = 0,
                        TotalSalary = d.NetAmount
                    });
                    otherRows.Add(new AgencyMonthlyExcelService.OtherSheetRow
                    {
                        SNo = sn,
                        SN = sn,
                        Name = d.GuardName,
                        Bank = d.BankAccountNumber,
                        IFSC = d.IFSCCode,
                        EmployeeCode = d.EmployeeCode,
                        Payment = d.NetAmount - d.OvertimeAmount,
                        OTHrsPayment = d.OvertimeAmount,
                        TotalPayment = d.NetAmount
                    });
                    sn++;
                }
            }
        }

        // Bill: use billId or first bill for month
        AgencyMonthlyExcelService.BillSheetRow? billRow = null;
        if (billId.HasValue)
        {
            var billResult = await _mediator.Send(new GetBillByIdQuery { Id = billId.Value }, cancellationToken);
            if (billResult.Success && billResult.Data != null)
            {
                var b = billResult.Data;
                billRow = new AgencyMonthlyExcelService.BillSheetRow
                {
                    BillNumber = b.BillNumber,
                    BillDate = b.BillDate,
                    ClientName = b.ClientName,
                    ClientAddress = b.SiteName,
                    TotalAmount = b.TotalAmount
                };
            }
        }
        if (billRow == null)
        {
            var billListResult = await _mediator.Send(new GetBillListQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                PageNumber = 1,
                PageSize = 1
            }, cancellationToken);
            var firstBill = billListResult.Data?.Items?.FirstOrDefault();
            if (firstBill != null)
            {
                var billResult = await _mediator.Send(new GetBillByIdQuery { Id = firstBill.Id }, cancellationToken);
                if (billResult.Success && billResult.Data != null)
                {
                    var b = billResult.Data;
                    billRow = new AgencyMonthlyExcelService.BillSheetRow
                    {
                        BillNumber = b.BillNumber,
                        BillDate = b.BillDate,
                        ClientName = b.ClientName,
                        ClientAddress = b.SiteName,
                        TotalAmount = b.TotalAmount
                    };
                }
            }
        }

        var bytes = service.Build(billRow, attendanceRows, wageRows, otherRows);
        var fileName = $"Agency_Monthly_{year}_{month:D2}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
