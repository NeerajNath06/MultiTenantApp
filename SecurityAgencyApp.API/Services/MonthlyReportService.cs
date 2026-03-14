using MediatR;
using SecurityAgencyApp.Application.Features.Attendance.Queries.GetAttendanceList;
using SecurityAgencyApp.Application.Features.Bills.Queries.GetBillById;
using SecurityAgencyApp.Application.Features.Bills.Queries.GetBillList;
using SecurityAgencyApp.Application.Features.Sites.Queries.GetSiteById;
using SecurityAgencyApp.Application.Features.TenantProfile.Queries.GetTenantProfile;
using SecurityAgencyApp.Application.Features.SiteRates.Queries.GetCurrentSiteRate;
using SecurityAgencyApp.Application.Features.Wages.Queries.GetWageList;
using SecurityAgencyApp.Application.Features.Wages.Queries.GetWageWithDetails;

namespace SecurityAgencyApp.API.Services;

/// <summary>Generates monthly reports (Bill, Attendance, Wages) for a site in Excel or PDF format.</summary>
public class MonthlyReportService
{
    private readonly IMediator _mediator;

    public MonthlyReportService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public enum ReportType { Bill, Attendance, Wages, Full }

    public async Task<(byte[] Content, string ContentType, string FileName)> GenerateAsync(
        Guid siteId,
        int year,
        int month,
        string format,
        ReportType reportType,
        CancellationToken cancellationToken = default)
    {
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be 1-12.");
        var fmt = (format ?? "Excel").Trim().ToLowerInvariant();
        if (fmt != "excel" && fmt != "pdf")
            fmt = "excel";

        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var siteResult = await _mediator.Send(new GetSiteByIdQuery { Id = siteId }, cancellationToken);
        if (!siteResult.Success || siteResult.Data == null)
            throw new InvalidOperationException("Site not found.");
        var siteName = SanitizeFileName(siteResult.Data.SiteName);
        var locationName = siteResult.Data.SiteName;
        var monthName = startDate.ToString("MMM");
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        var profileResult = await _mediator.Send(new GetTenantProfileQuery(), cancellationToken);
        var header = ExportReportHeaderBuilder.BuildWithDateRange(profileResult.Data, "Monthly Report", startDate, endDate);
        var excelService = new AgencyMonthlyExcelService(header, locationName, year, month);

        AgencyMonthlyExcelService.BillSheetRow? billRow = null;
        var attendanceRows = new List<AgencyMonthlyExcelService.AttendanceDayRow>();
        var wageRows = new List<AgencyMonthlyExcelService.WageSheetRow>();
        var otherRows = new List<AgencyMonthlyExcelService.OtherSheetRow>();

        if (reportType == ReportType.Bill || reportType == ReportType.Full)
        {
            var billListResult = await _mediator.Send(new GetBillListQuery
            {
                SiteId = siteId,
                StartDate = startDate,
                EndDate = endDate,
                SortBy = "billdate",
                SortDirection = "desc",
                PageNumber = 1,
                PageSize = 1
            }, cancellationToken);
            var firstBill = billListResult.Data?.Items?.FirstOrDefault();
            if (firstBill != null)
            {
                var billDetailResult = await _mediator.Send(new GetBillByIdQuery { Id = firstBill.Id }, cancellationToken);
                if (billDetailResult.Success && billDetailResult.Data != null)
                {
                    var b = billDetailResult.Data;
                    var items = new List<AgencyMonthlyExcelService.BillLineItem>();
                    if (b.Items != null && b.Items.Count > 0)
                    {
                        int sn = 1;
                        foreach (var it in b.Items)
                        {
                            // Auto-generated bills: one row per guard, Quantity = days. Show NoOfPersons=1, TotalDuty=days.
                            // Manual bills: Quantity can be persons; then TotalDuty = same (backward compat).
                            var totalDuty = it.Quantity;
                            var noOfPersons = 1;
                            if (totalDuty > 31)
                            {
                                noOfPersons = it.Quantity;
                                totalDuty = it.Quantity;
                            }
                            items.Add(new AgencyMonthlyExcelService.BillLineItem
                            {
                                SrNo = sn++,
                                NoOfPersons = noOfPersons,
                                Particulars = it.ItemName,
                                TotalDuty = totalDuty,
                                Rate = it.UnitPrice,
                                Amount = it.TotalAmount
                            });
                        }
                    }
                    // Rate snapshot fallback (if bill has no items)
                    if (items.Count == 0)
                    {
                        var rateRes = await _mediator.Send(new GetCurrentSiteRateQuery { SiteId = siteId, AsOfDate = startDate }, cancellationToken);
                        if (rateRes.Success && rateRes.Data != null)
                        {
                            items.Add(new AgencyMonthlyExcelService.BillLineItem
                            {
                                SrNo = 1,
                                NoOfPersons = 0,
                                Particulars = "Present-based billing (auto)",
                                TotalDuty = 0,
                                Rate = rateRes.Data.RateAmount,
                                Amount = b.TotalAmount
                            });
                        }
                    }
                    billRow = new AgencyMonthlyExcelService.BillSheetRow
                    {
                        BillNumber = b.BillNumber,
                        BillDate = b.BillDate,
                        ClientName = b.ClientName,
                        ClientAddress = b.SiteName,
                        TotalAmount = b.TotalAmount,
                        PeriodLabel = $"{startDate:dd-MM-yyyy} to {endDate:dd-MM-yyyy}",
                        Items = items
                    };
                }
            }
        }

        if (reportType == ReportType.Attendance || reportType == ReportType.Full)
        {
            var attQuery = new GetAttendanceListQuery
            {
                SiteId = siteId,
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
            int sn = 1;
            foreach (var g in guardDays)
            {
                attendanceRows.Add(new AgencyMonthlyExcelService.AttendanceDayRow
                {
                    SNo = sn++,
                    GuardName = g.GuardName,
                    Designation = "S/G",
                    DayStatus = g.Days
                });
            }
        }

        if (reportType == ReportType.Wages || reportType == ReportType.Full)
        {
            decimal epfPercent = 12m;
            decimal esicPercent = 0.75m;
            decimal epfWageCap = 15000m;
            var rateRes = await _mediator.Send(new GetCurrentSiteRateQuery { SiteId = siteId, AsOfDate = startDate }, cancellationToken);
            if (rateRes.Success && rateRes.Data != null)
            {
                if (rateRes.Data.EpfPercent.HasValue) epfPercent = rateRes.Data.EpfPercent.Value;
                if (rateRes.Data.EsicPercent.HasValue) esicPercent = rateRes.Data.EsicPercent.Value;
                if (rateRes.Data.EpfWageCap.HasValue && rateRes.Data.EpfWageCap.Value > 0) epfWageCap = rateRes.Data.EpfWageCap.Value;
            }

            var wageListResult = await _mediator.Send(new GetWageListQuery
            {
                SiteId = siteId,
                PeriodStart = startDate,
                PeriodEnd = endDate,
                SortBy = "paymentdate",
                SortDirection = "desc",
                PageNumber = 1,
                PageSize = 1
            }, cancellationToken);
            var wageId = wageListResult.Data?.Items?.FirstOrDefault()?.Id;
            if (wageId.HasValue)
            {
                var wageDetailResult = await _mediator.Send(new GetWageWithDetailsQuery { WageId = wageId.Value, SiteId = siteId }, cancellationToken);
                if (wageDetailResult.Success && wageDetailResult.Data?.Details != null)
                {
                    int sn = 1;
                    foreach (var d in wageDetailResult.Data.Details)
                    {
                        var earnWages = d.BasicAmount;
                        var epfWagesAtCap = Math.Min(earnWages, epfWageCap);
                        var epfDeduction = Math.Round(epfWagesAtCap * (epfPercent / 100m), 2);
                        var esicDeduction = Math.Round(earnWages * (esicPercent / 100m), 2);
                        var totalDeduction = epfDeduction + esicDeduction;
                        var totalPayment = earnWages - totalDeduction + (d.OvertimeAmount);
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
                            EarnWages = earnWages,
                            EpfWagesAt15000 = epfWagesAtCap,
                            EmployeeShareEPF12 = epfDeduction,
                            EmployeeShareESIC075 = esicDeduction,
                            TotalDeduction = totalDeduction,
                            OTPayment = d.OvertimeAmount,
                            EDPayment = 0,
                            TotalSalary = totalPayment
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
        }

        var baseFileName = $"{siteName}_{monthName}_{year}_{timestamp}";

        if (fmt == "pdf")
        {
            byte[] pdfBytes;
            if (reportType == ReportType.Bill)
                pdfBytes = MonthlyReportPdfBuilder.BuildBillOnlyPdf(header, locationName, year, month, billRow);
            else if (reportType == ReportType.Attendance)
                pdfBytes = MonthlyReportPdfBuilder.BuildAttendanceOnlyPdf(header, locationName, year, month, attendanceRows);
            else if (reportType == ReportType.Wages)
                pdfBytes = MonthlyReportPdfBuilder.BuildWagesOnlyPdf(header, locationName, year, month, wageRows);
            else
                pdfBytes = MonthlyReportPdfBuilder.BuildFullReportBytes(header, locationName, year, month, billRow, attendanceRows, wageRows);
            return (pdfBytes, "application/pdf", baseFileName + ".pdf");
        }

        byte[] excelBytes;
        if (reportType == ReportType.Bill)
            excelBytes = excelService.Build(billRow, new List<AgencyMonthlyExcelService.AttendanceDayRow>(), new List<AgencyMonthlyExcelService.WageSheetRow>(), new List<AgencyMonthlyExcelService.OtherSheetRow>(), includeBill: true, includeAttendance: false, includeWages: false, includeOtherSheet: false);
        else if (reportType == ReportType.Attendance)
            excelBytes = excelService.Build(null, attendanceRows, new List<AgencyMonthlyExcelService.WageSheetRow>(), new List<AgencyMonthlyExcelService.OtherSheetRow>(), includeBill: false, includeAttendance: true, includeWages: false, includeOtherSheet: false);
        else if (reportType == ReportType.Wages)
            excelBytes = excelService.Build(null, new List<AgencyMonthlyExcelService.AttendanceDayRow>(), wageRows, otherRows, includeBill: false, includeAttendance: false, includeWages: true, includeOtherSheet: false);
        else
            excelBytes = excelService.Build(billRow, attendanceRows, wageRows, otherRows, includeOtherSheet: false);
        return (excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", baseFileName + ".xlsx");
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Site";
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(name.Select(c => Array.IndexOf(invalid, c) >= 0 ? '_' : c).ToArray()).Trim();
        return string.IsNullOrEmpty(sanitized) ? "Site" : sanitized;
    }
}
