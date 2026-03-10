using ClosedXML.Excel;

namespace SecurityAgencyApp.API.Services;

/// <summary>Builds agency monthly Excel (4 sheets: BILL, ATTENDANCE, Wages, Other) like reference format.</summary>
public class AgencyMonthlyExcelService
{
    private readonly ExportReportHeader? _header;
    private readonly string? _locationName;
    private readonly int _year;
    private readonly int _month;
    private readonly string _monthLabel;

    public AgencyMonthlyExcelService(ExportReportHeader? header, string? locationName, int year, int month)
    {
        _header = header;
        _locationName = locationName ?? "Site";
        _year = year;
        _month = month;
        _monthLabel = new DateTime(year, month, 1).ToString("MMM-yy");
    }

    /// <summary>One row for bill header: agency + client + bill info.</summary>
    public class BillSheetRow
    {
        public string? BillNumber { get; set; }
        public DateTime? BillDate { get; set; }
        public string? ClientName { get; set; }
        public string? ClientAddress { get; set; }
        public string? ClientCity { get; set; }
        public decimal TotalAmount { get; set; }
        public string? ServiceDescription { get; set; }
        public string? PeriodLabel { get; set; } // e.g. "01-02-2026 to 31-02-2026"
        public List<BillLineItem> Items { get; set; } = new();
    }

    public class BillLineItem
    {
        public int SrNo { get; set; }
        public int NoOfPersons { get; set; }
        public string Particulars { get; set; } = string.Empty;
        public decimal TotalDuty { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>Attendance day-wise: GuardName, Designation, then day1..day31 (P/A).</summary>
    public class AttendanceDayRow
    {
        public int SNo { get; set; }
        public string GuardName { get; set; } = string.Empty;
        public string Designation { get; set; } = "S/G";
        public Dictionary<int, string> DayStatus { get; set; } = new(); // 1..31 -> "P" or "A"
    }

    /// <summary>Wage row matching reference columns.</summary>
    public class WageSheetRow
    {
        public int SNo { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Designation { get; set; } = "";
        public string? UAN { get; set; }
        public decimal BasicPerDay { get; set; }
        public int Attendance { get; set; }
        public decimal OTHrs { get; set; }
        public decimal ED { get; set; }
        public decimal EarnWages { get; set; }
        public decimal EpfWagesAt15000 { get; set; }
        public decimal EmployeeShareEPF12 { get; set; }
        public decimal EmployeeShareESIC075 { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal OTPayment { get; set; }
        public decimal EDPayment { get; set; }
        public decimal TotalSalary { get; set; }
    }

    /// <summary>Other (payment) sheet row.</summary>
    public class OtherSheetRow
    {
        public int SNo { get; set; }
        public int SN { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Bank { get; set; }
        public string? IFSC { get; set; }
        public string? EmployeeCode { get; set; }
        public decimal Payment { get; set; }
        public decimal OTHrsPayment { get; set; }
        public decimal TotalPayment { get; set; }
    }

    public byte[] Build(
        BillSheetRow? billRow,
        IReadOnlyList<AttendanceDayRow> attendanceRows,
        IReadOnlyList<WageSheetRow> wageRows,
        IReadOnlyList<OtherSheetRow> otherRows)
    {
        return Build(billRow, attendanceRows, wageRows, otherRows, includeOtherSheet: true);
    }

    /// <summary>Build workbook with optional sheets. For "full" report use includeOtherSheet: false (3 sheets: Bill, Attendance, Wages).</summary>
    public byte[] Build(
        BillSheetRow? billRow,
        IReadOnlyList<AttendanceDayRow> attendanceRows,
        IReadOnlyList<WageSheetRow> wageRows,
        IReadOnlyList<OtherSheetRow> otherRows,
        bool includeBill = true,
        bool includeAttendance = true,
        bool includeWages = true,
        bool includeOtherSheet = true)
    {
        using var wb = new XLWorkbook();
        if (includeBill) AddBillSheet(wb, billRow);
        if (includeAttendance) AddAttendanceSheet(wb, attendanceRows);
        if (includeWages) AddWagesSheet(wb, wageRows);
        if (includeOtherSheet) AddOtherSheet(wb, otherRows);
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private void AddBillSheet(XLWorkbook wb, BillSheetRow? bill)
    {
        var ws = wb.Worksheets.Add("BILL");
        ws.Style.Font.FontName = "Calibri";
        ws.Style.Font.FontSize = 11;
        int row = 1;
        // Agency name top center, bold
        ws.Cell(row, 1).Value = _header?.AgencyName ?? "Agency";
        ws.Range(row, 1, row, 8).Merge().Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 14;
        ws.Range(row, 1, row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        row++;
        if (!string.IsNullOrWhiteSpace(_header?.FormattedAddress))
        {
            ws.Cell(row, 1).Value = _header.FormattedAddress;
            ws.Range(row, 1, row, 8).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row++;
        }
        row++;
        // Identification row: include month like "Jan-26"
        ws.Cell(row, 1).Value = "PF No. - " + (_header?.ReportTitle ?? "");
        ws.Cell(row, 3).Value = "GSTIN : " + (_header?.Phone ?? "");
        ws.Cell(row, 5).Value = "SAC-998525";
        ws.Cell(row, 7).Value = "PAN NO. - ";
        ws.Cell(row, 8).Value = _monthLabel;
        row++;
        ws.Cell(row, 1).Value = "ESIC NO.- ";
        ws.Cell(row, 3).Value = "PSRA - ";
        ws.Cell(row, 5).Value = "";
        ws.Cell(row, 7).Value = "Bid/RA/PR No. : ";
        row += 2;
        ws.Cell(row, 1).Value = "SERVICE DETAILS";
        ws.Range(row, 1, row, 8).Merge().Style.Font.Bold = true;
        ws.Range(row, 1, row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        row++;
        ws.Cell(row, 1).Value = "SERVICE PROVIDER";
        ws.Cell(row, 5).Value = "SERVICE RECEIVER";
        ws.Range(row, 1, row, 4).Style.Font.Bold = true;
        ws.Range(row, 5, row, 8).Style.Font.Bold = true;
        row++;
        ws.Cell(row, 1).Value = _header?.AgencyName ?? "";
        ws.Cell(row, 5).Value = "To";
        row++;
        ws.Cell(row, 1).Value = _header?.FormattedAddress ?? "";
        ws.Cell(row, 5).Value = bill?.ClientName ?? _locationName;
        row++;
        ws.Cell(row, 1).Value = _header?.City + " " + _header?.State ?? "";
        ws.Cell(row, 5).Value = bill?.ClientAddress ?? "";
        row++;
        ws.Cell(row, 1).Value = "GST NO : ";
        ws.Cell(row, 5).Value = bill?.ClientCity ?? "";
        row++;
        ws.Cell(row, 1).Value = "Tender No. :- ";
        ws.Cell(row, 5).Value = "PIN CODE - ";

        // Invoice meta (like template)
        row += 2;
        ws.Cell(row, 1).Value = "INVOICE NO. -";
        ws.Cell(row, 3).Value = bill?.BillNumber ?? "";
        ws.Cell(row, 5).Value = "DATED -";
        ws.Cell(row, 6).Value = bill?.BillDate?.ToString("dd-MM-yyyy") ?? "";
        ws.Range(row, 1, row, 2).Merge();
        ws.Range(row, 3, row, 4).Merge();
        ws.Range(row, 6, row, 8).Merge();
        row++;
        ws.Cell(row, 1).Value = "BILL FOR SECURITY";
        ws.Range(row, 1, row, 2).Merge();
        ws.Cell(row, 3).Value = bill?.PeriodLabel ?? "";
        ws.Range(row, 3, row, 6).Merge();
        ws.Cell(row, 7).Value = "RECEIVER STATE -";
        ws.Range(row, 7, row, 8).Merge();

        // Bordered table
        row += 2;
        var headerRow = row;
        ws.Cell(row, 1).Value = "Sr. No";
        ws.Cell(row, 2).Value = "NO OF PERSONS";
        ws.Cell(row, 3).Value = "PARTICULARS";
        ws.Cell(row, 4).Value = "TOTAL DUTY";
        ws.Cell(row, 5).Value = "RATE";
        ws.Cell(row, 6).Value = "AMOUNT";
        ws.Range(row, 1, row, 6).Style.Font.Bold = true;
        ws.Range(row, 1, row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Range(row, 1, row, 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Row(row).Height = 22;

        row++;
        var items = bill?.Items ?? new List<BillLineItem>();
        if (items.Count == 0)
        {
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "";
            ws.Cell(row, 3).Value = "No items";
            ws.Range(row, 3, row, 6).Merge();
            row++;
        }
        else
        {
            foreach (var it in items.OrderBy(i => i.SrNo))
            {
                ws.Cell(row, 1).Value = it.SrNo;
                ws.Cell(row, 2).Value = it.NoOfPersons;
                ws.Cell(row, 3).Value = it.Particulars;
                ws.Cell(row, 4).Value = it.TotalDuty;
                ws.Cell(row, 5).Value = it.Rate;
                ws.Cell(row, 6).Value = it.Amount;
                row++;
            }
        }

        // Total row
        ws.Cell(row, 1).Value = "TOTAL";
        ws.Range(row, 1, row, 5).Merge().Style.Font.Bold = true;
        ws.Cell(row, 6).Value = bill?.TotalAmount ?? 0m;
        ws.Cell(row, 6).Style.Font.Bold = true;
        ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";

        var tableRange = ws.Range(headerRow, 1, row, 6);
        ApplyGridBorders(tableRange);
        ws.Range(headerRow + 1, 4, row, 6).Style.NumberFormat.Format = "#,##0.00";
        ws.Columns(1, 6).AdjustToContents();
        ws.Column(3).Width = Math.Max(ws.Column(3).Width, 25);
    }

    private void AddAttendanceSheet(XLWorkbook wb, IReadOnlyList<AttendanceDayRow> rows)
    {
        var ws = wb.Worksheets.Add("ATTENDANCE ");
        ws.Style.Font.FontName = "Calibri";
        ws.Style.Font.FontSize = 10;
        int row = 1;
        // Agency name top center, bold
        ws.Cell(row, 1).Value = _header?.AgencyName ?? "Agency";
        ws.Range(row, 1, row, 35).Merge().Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        row++;
        // Left: LOCATION, Right: ATTENDANCE FOR THE MONTH OF JAN -26
        var monthLabelUpper = new DateTime(_year, _month, 1).ToString("MMM").ToUpperInvariant() + " -" + new DateTime(_year, _month, 1).ToString("yy");
        ws.Cell(row, 1).Value = "LOCATION :-  " + _locationName;
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 20).Value = "ATTENDANCE FOR THE MONTH OF " + monthLabelUpper;
        ws.Range(row, 20, row, 35).Merge().Style.Font.Bold = true;
        ws.Cell(row, 20).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        row += 2;
        int daysInMonth = DateTime.DaysInMonth(_year, _month);
        // Headers: S NO., NAME, TYPE, 1..31, Total
        ws.Cell(row, 1).Value = "S NO.";
        ws.Cell(row, 2).Value = "NAME";
        ws.Cell(row, 3).Value = "TYPE";
        for (int d = 1; d <= 31; d++)
            ws.Cell(row, 3 + d).Value = d;
        ws.Cell(row, 35).Value = "Total";
        ws.Range(row, 1, row, 35).Style.Font.Bold = true;
        ws.Row(row).Height = 20;
        ws.Range(row, 1, row, 35).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Range(row, 1, row, 35).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        row++;
        var dataStartRow = row;
        foreach (var r in rows)
        {
            ws.Cell(row, 1).Value = r.SNo;
            ws.Cell(row, 2).Value = r.GuardName;
            ws.Cell(row, 3).Value = r.Designation;
            int totalP = 0;
            for (int d = 1; d <= daysInMonth; d++)
            {
                var s = r.DayStatus.TryGetValue(d, out var v) ? v : "";
                if (string.Equals(s, "P", StringComparison.OrdinalIgnoreCase)) totalP++;
                ws.Cell(row, 3 + d).Value = s;
            }
            for (int d = daysInMonth + 1; d <= 31; d++)
                ws.Cell(row, 3 + d).Value = "";
            ws.Cell(row, 35).Value = totalP;
            ws.Cell(row, 35).Style.Font.Bold = true;
            row++;
        }
        // Grand total row
        if (rows.Count > 0)
        {
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "";
            ws.Cell(row, 3).Value = "";
            for (int d = 1; d <= 31; d++)
                ws.Cell(row, 3 + d).Value = "";
            var grandTotal = rows.Sum(r => r.DayStatus.Count(kv => string.Equals(kv.Value, "P", StringComparison.OrdinalIgnoreCase)));
            ws.Cell(row, 35).Value = grandTotal;
            ws.Cell(row, 35).Style.Font.Bold = true;
            row++;
        }
        var lastRow = Math.Max(dataStartRow, row - 1);
        var lastCol = 35;
        var grid = ws.Range(dataStartRow - 1, 1, lastRow, lastCol);
        ApplyGridBorders(grid);
        ws.Range(dataStartRow, 1, lastRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Range(dataStartRow, 3, lastRow, lastCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Range(dataStartRow, 3, lastRow, lastCol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Columns(1, lastCol).AdjustToContents();
        ws.Column(2).Width = Math.Max(ws.Column(2).Width, 25);
    }

    private void AddWagesSheet(XLWorkbook wb, IReadOnlyList<WageSheetRow> rows)
    {
        var ws = wb.Worksheets.Add("Wages");
        ws.Style.Font.FontName = "Calibri";
        ws.Style.Font.FontSize = 10;
        int row = 1;
        // Agency name top center, bold
        ws.Cell(row, 1).Value = (_header?.AgencyName ?? "Agency").ToUpperInvariant();
        ws.Range(row, 1, row, 17).Merge().Style.Font.Bold = true;
        ws.Range(row, 1, row, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        row++;
        if (!string.IsNullOrWhiteSpace(_header?.FormattedAddress))
        {
            ws.Cell(row, 1).Value = _header.FormattedAddress;
            ws.Range(row, 1, row, 17).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;
        }
        else
            row += 2;
        // Left: Unit : location, Right: Wages Sheet for the Month of JAN -26
        var monthLabelUpper = new DateTime(_year, _month, 1).ToString("MMM").ToUpperInvariant() + " -" + new DateTime(_year, _month, 1).ToString("yy");
        ws.Cell(row, 1).Value = "Unit : " + _locationName;
        ws.Range(row, 1, row, 8).Merge().Style.Font.Bold = true;
        ws.Cell(row, 10).Value = "Wages Sheet for the Month of " + monthLabelUpper;
        ws.Range(row, 10, row, 17).Merge().Style.Font.Bold = true;
        ws.Cell(row, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        row += 2;
        ws.Cell(row, 1).Value = "S No.";
        ws.Cell(row, 2).Value = "Name of Employee";
        ws.Cell(row, 3).Value = "Designation";
        ws.Cell(row, 4).Value = "UAN NO.";
        ws.Cell(row, 5).Value = "Basic/ Per Day";
        ws.Cell(row, 6).Value = "Attendance";
        ws.Cell(row, 7).Value = "OT Hrs";
        ws.Cell(row, 8).Value = "ED";
        ws.Cell(row, 9).Value = "Earn Wages";
        ws.Cell(row, 10).Value = "Epf Wages @ 15000";
        ws.Cell(row, 11).Value = "Employee Share on EPF @ 12 %";
        ws.Cell(row, 12).Value = "Employee Share on ESIC @ .75 %";
        ws.Cell(row, 13).Value = "Total Deduction";
        ws.Cell(row, 14).Value = "OT Hrs Payment";
        ws.Cell(row, 15).Value = "ED Payment";
        ws.Cell(row, 16).Value = "Total Salary";
        ws.Cell(row, 17).Value = "Signature";
        ws.Range(row, 1, row, 17).Style.Font.Bold = true;
        ws.Range(row, 1, row, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Range(row, 1, row, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Row(row).Height = 28;
        row++;
        var dataStartRow = row;
        foreach (var r in rows)
        {
            ws.Cell(row, 1).Value = r.SNo;
            ws.Cell(row, 2).Value = r.Name;
            ws.Cell(row, 3).Value = r.Designation;
            ws.Cell(row, 4).Value = r.UAN ?? "";
            ws.Cell(row, 5).Value = r.BasicPerDay;
            ws.Cell(row, 6).Value = r.Attendance;
            ws.Cell(row, 7).Value = r.OTHrs;
            ws.Cell(row, 8).Value = r.ED;
            ws.Cell(row, 9).Value = r.EarnWages;
            ws.Cell(row, 10).Value = r.EpfWagesAt15000;
            ws.Cell(row, 11).Value = r.EmployeeShareEPF12;
            ws.Cell(row, 12).Value = r.EmployeeShareESIC075;
            ws.Cell(row, 13).Value = r.TotalDeduction;
            ws.Cell(row, 14).Value = r.OTPayment;
            ws.Cell(row, 15).Value = r.EDPayment;
            ws.Cell(row, 16).Value = r.TotalSalary;
            row++;
        }
        var lastRow = Math.Max(dataStartRow, row - 1);
        var grid = ws.Range(dataStartRow - 1, 1, lastRow, 16);
        ApplyGridBorders(grid);
        ws.Range(dataStartRow, 1, lastRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Range(dataStartRow, 5, lastRow, 16).Style.NumberFormat.Format = "#,##0.00";
        ws.Range(dataStartRow, 6, lastRow, 6).Style.NumberFormat.NumberFormatId = 0;
        ws.Columns(1, 16).AdjustToContents();
        ws.Column(2).Width = Math.Max(ws.Column(2).Width, 22);
    }

    private void AddOtherSheet(XLWorkbook wb, IReadOnlyList<OtherSheetRow> rows)
    {
        var ws = wb.Worksheets.Add("OTHER");
        ws.Style.Font.FontName = "Calibri";
        ws.Style.Font.FontSize = 10;
        int row = 1;
        ws.Cell(row, 1).Value = (_header?.AgencyName ?? "Agency").ToUpperInvariant();
        ws.Range(row, 1, row, 9).Merge().Style.Font.Bold = true;
        row++;
        ws.Cell(row, 1).Value = _locationName;
        ws.Range(row, 1, row, 9).Merge();
        row++;
        ws.Cell(row, 1).Value = "PAYMENT FOR THE MONTH OF " + _monthLabel.ToUpperInvariant();
        ws.Range(row, 1, row, 9).Merge();
        row++;
        ws.Cell(row, 1).Value = "S NO.";
        ws.Cell(row, 2).Value = "S N";
        ws.Cell(row, 3).Value = "NAME";
        ws.Cell(row, 4).Value = "BANK";
        ws.Cell(row, 5).Value = "IFSC";
        ws.Cell(row, 6).Value = "EMPLOYEE CODE";
        ws.Cell(row, 7).Value = "PAYMENT";
        ws.Cell(row, 8).Value = "OT HRS PAYMENT";
        ws.Cell(row, 9).Value = "TOTAL PAYMENT";
        ws.Range(row, 1, row, 9).Style.Font.Bold = true;
        ws.Range(row, 1, row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        row++;
        var dataStartRow = row;
        foreach (var r in rows)
        {
            ws.Cell(row, 1).Value = r.SNo;
            ws.Cell(row, 2).Value = r.SN;
            ws.Cell(row, 3).Value = r.Name;
            ws.Cell(row, 4).Value = r.Bank ?? "";
            ws.Cell(row, 5).Value = r.IFSC ?? "";
            ws.Cell(row, 6).Value = r.EmployeeCode ?? "";
            ws.Cell(row, 7).Value = r.Payment;
            ws.Cell(row, 8).Value = r.OTHrsPayment;
            ws.Cell(row, 9).Value = r.TotalPayment;
            row++;
        }
        var lastRow = Math.Max(dataStartRow, row - 1);
        var grid = ws.Range(dataStartRow - 1, 1, lastRow, 9);
        ApplyGridBorders(grid);
        ws.Range(dataStartRow, 7, lastRow, 9).Style.NumberFormat.Format = "#,##0.00";
        ws.Columns(1, 9).AdjustToContents();
    }

    private static void ApplyGridBorders(IXLRange range)
    {
        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        range.Style.Border.OutsideBorderColor = XLColor.Black;
        range.Style.Border.InsideBorderColor = XLColor.Black;
    }
}
