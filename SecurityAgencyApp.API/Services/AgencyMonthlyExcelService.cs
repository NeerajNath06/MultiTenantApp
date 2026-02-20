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
        _monthLabel = new DateTime(year, month, 1).ToString("MMM - yy");
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
        using var wb = new XLWorkbook();
        AddBillSheet(wb, billRow);
        AddAttendanceSheet(wb, attendanceRows);
        AddWagesSheet(wb, wageRows);
        AddOtherSheet(wb, otherRows);
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private void AddBillSheet(XLWorkbook wb, BillSheetRow? bill)
    {
        var ws = wb.Worksheets.Add("BILL");
        int row = 1;
        ws.Cell(row, 1).Value = _header?.AgencyName ?? "Agency";
        ws.Range(row, 1, row, 8).Merge().Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 14;
        row++;
        if (!string.IsNullOrWhiteSpace(_header?.FormattedAddress))
        {
            ws.Cell(row, 1).Value = _header.FormattedAddress;
            ws.Range(row, 1, row, 8).Merge();
            row++;
        }
        row += 2;
        ws.Cell(row, 1).Value = "PF No. - " + (_header?.ReportTitle ?? "");
        ws.Cell(row, 3).Value = "GSTIN : " + (_header?.Phone ?? "");
        ws.Cell(row, 5).Value = "SAC-998525";
        ws.Cell(row, 7).Value = "PAN NO. - ";
        row++;
        ws.Cell(row, 1).Value = "ESIC NO.- ";
        ws.Cell(row, 3).Value = "PSRA - ";
        ws.Cell(row, 5).Value = "";
        ws.Cell(row, 7).Value = "Bid/RA/PR No. : ";
        row += 2;
        ws.Cell(row, 1).Value = "SERVICE DETAILS";
        ws.Range(row, 1, row, 8).Merge().Style.Font.Bold = true;
        row++;
        ws.Cell(row, 1).Value = "SERVICE PROVIDER";
        ws.Cell(row, 5).Value = "SERVICE RECEIVER";
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
        ws.Columns().AdjustToContents();
    }

    private void AddAttendanceSheet(XLWorkbook wb, IReadOnlyList<AttendanceDayRow> rows)
    {
        var ws = wb.Worksheets.Add("ATTENDANCE ");
        int row = 1;
        ws.Cell(row, 1).Value = _header?.AgencyName ?? "Agency";
        ws.Range(row, 1, row, 35).Merge().Style.Font.Bold = true;
        row++;
        ws.Cell(row, 1).Value = "LOCATION :-  " + _locationName;
        ws.Range(row, 1, row, 35).Merge();
        row += 2;
        ws.Cell(row, 1).Value = "S NO.";
        ws.Cell(row, 2).Value = "NAME";
        ws.Cell(row, 3).Value = "TYPE";
        for (int d = 1; d <= 31; d++)
            ws.Cell(row, 3 + d).Value = d;
        ws.Range(row, 1, row, 34).Style.Font.Bold = true;
        row++;
        int daysInMonth = DateTime.DaysInMonth(_year, _month);
        foreach (var r in rows)
        {
            ws.Cell(row, 1).Value = r.SNo;
            ws.Cell(row, 2).Value = r.GuardName;
            ws.Cell(row, 3).Value = r.Designation;
            for (int d = 1; d <= daysInMonth; d++)
                ws.Cell(row, 3 + d).Value = r.DayStatus.TryGetValue(d, out var s) ? s : "";
            row++;
        }
        ws.Columns().AdjustToContents();
    }

    private void AddWagesSheet(XLWorkbook wb, IReadOnlyList<WageSheetRow> rows)
    {
        var ws = wb.Worksheets.Add("Wages");
        int row = 1;
        ws.Cell(row, 1).Value = (_header?.AgencyName ?? "Agency").ToUpperInvariant();
        ws.Range(row, 1, row, 17).Merge().Style.Font.Bold = true;
        row += 3;
        if (!string.IsNullOrWhiteSpace(_header?.FormattedAddress))
        {
            ws.Cell(row, 1).Value = _header.FormattedAddress;
            ws.Range(row, 1, row, 17).Merge();
            row += 2;
        }
        row += 5;
        ws.Cell(row, 1).Value = "Unit : " + _locationName;
        ws.Range(row, 1, row, 17).Merge();
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
        row++;
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
        ws.Columns().AdjustToContents();
    }

    private void AddOtherSheet(XLWorkbook wb, IReadOnlyList<OtherSheetRow> rows)
    {
        var ws = wb.Worksheets.Add("OTHER");
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
        row++;
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
        ws.Columns().AdjustToContents();
    }
}
