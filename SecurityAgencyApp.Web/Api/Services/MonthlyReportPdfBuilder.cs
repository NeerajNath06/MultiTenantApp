using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SecurityAgencyApp.API.Services;

/// <summary>Builds a multi-page PDF for monthly report (Bill, Attendance, Wages) matching Excel structure.</summary>
public class MonthlyReportPdfBuilder
{
    private readonly ExportReportHeader? _header;
    private readonly string _locationName;
    private readonly int _year;
    private readonly int _month;
    private readonly string _monthLabel;

    public MonthlyReportPdfBuilder(ExportReportHeader? header, string locationName, int year, int month)
    {
        _header = header;
        _locationName = locationName ?? "Site";
        _year = year;
        _month = month;
        _monthLabel = new DateTime(year, month, 1).ToString("MMM-yy");
    }

    public static void BuildFullReport(
        ExportReportHeader? header,
        string locationName,
        int year,
        int month,
        AgencyMonthlyExcelService.BillSheetRow? billRow,
        IReadOnlyList<AgencyMonthlyExcelService.AttendanceDayRow> attendanceRows,
        IReadOnlyList<AgencyMonthlyExcelService.WageSheetRow> wageRows,
        Stream outputStream)
    {
        var builder = new MonthlyReportPdfBuilder(header, locationName, year, month);
        Document.Create(container =>
        {
            builder.AddBillPage(container, billRow);
            builder.AddAttendancePage(container, attendanceRows);
            builder.AddWagesPage(container, wageRows);
        }).GeneratePdf(outputStream);
    }

    public static byte[] BuildFullReportBytes(
        ExportReportHeader? header,
        string locationName,
        int year,
        int month,
        AgencyMonthlyExcelService.BillSheetRow? billRow,
        IReadOnlyList<AgencyMonthlyExcelService.AttendanceDayRow> attendanceRows,
        IReadOnlyList<AgencyMonthlyExcelService.WageSheetRow> wageRows)
    {
        using var ms = new MemoryStream();
        BuildFullReport(header, locationName, year, month, billRow, attendanceRows, wageRows, ms);
        return ms.ToArray();
    }

    public static byte[] BuildBillOnlyPdf(ExportReportHeader? header, string locationName, int year, int month, AgencyMonthlyExcelService.BillSheetRow? billRow)
    {
        using var ms = new MemoryStream();
        var builder = new MonthlyReportPdfBuilder(header, locationName, year, month);
        Document.Create(container => builder.AddBillPage(container, billRow)).GeneratePdf(ms);
        return ms.ToArray();
    }

    public static byte[] BuildAttendanceOnlyPdf(ExportReportHeader? header, string locationName, int year, int month, IReadOnlyList<AgencyMonthlyExcelService.AttendanceDayRow> rows)
    {
        using var ms = new MemoryStream();
        var builder = new MonthlyReportPdfBuilder(header, locationName, year, month);
        Document.Create(container => builder.AddAttendancePage(container, rows)).GeneratePdf(ms);
        return ms.ToArray();
    }

    public static byte[] BuildWagesOnlyPdf(ExportReportHeader? header, string locationName, int year, int month, IReadOnlyList<AgencyMonthlyExcelService.WageSheetRow> rows)
    {
        using var ms = new MemoryStream();
        var builder = new MonthlyReportPdfBuilder(header, locationName, year, month);
        Document.Create(container => builder.AddWagesPage(container, rows)).GeneratePdf(ms);
        return ms.ToArray();
    }

    private void AddBillPage(IDocumentContainer container, AgencyMonthlyExcelService.BillSheetRow? bill)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(1.5f, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(9));

            page.Header().Column(col =>
            {
                col.Item().AlignCenter().Text(_header?.AgencyName ?? "Agency").Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
                if (!string.IsNullOrWhiteSpace(_header?.FormattedAddress))
                    col.Item().AlignCenter().Text(_header.FormattedAddress).FontSize(8).FontColor(Colors.Grey.Medium);
                col.Item().PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text("BILL - " + _monthLabel).Bold().FontSize(12);
                });
            });

            page.Content().Column(col =>
            {
                col.Item().Text("SERVICE PROVIDER").Bold();
                col.Item().Text(_header?.AgencyName ?? "");
                col.Item().Text(_header?.FormattedAddress ?? "");
                col.Item().PaddingTop(8).Text("SERVICE RECEIVER").Bold();
                col.Item().Text("To: " + (bill?.ClientName ?? _locationName));
                col.Item().Text(bill?.ClientAddress ?? "");
                if (bill != null)
                {
                    col.Item().PaddingTop(12).Row(r =>
                    {
                        r.RelativeItem().Text("Bill No: " + bill.BillNumber);
                        r.RelativeItem().Text("Date: " + (bill.BillDate?.ToString("dd-MMM-yyyy") ?? ""));
                        r.RelativeItem().Text("Total: " + bill.TotalAmount.ToString("N2"));
                    });
                }

                // Invoice table (bordered like template)
                var items = bill?.Items ?? new List<AgencyMonthlyExcelService.BillLineItem>();
                col.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(def =>
                    {
                        def.ConstantColumn(40);
                        def.ConstantColumn(60);
                        def.RelativeColumn();
                        def.ConstantColumn(60);
                        def.ConstantColumn(60);
                        def.ConstantColumn(70);
                    });

                    void HeaderCell(string text) =>
                        table.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text(text).Bold();

                    HeaderCell("Sr. No");
                    HeaderCell("Persons");
                    HeaderCell("Particulars");
                    HeaderCell("Total Duty");
                    HeaderCell("Rate");
                    HeaderCell("Amount");

                    if (items.Count == 0)
                    {
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(4).Text("");
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(4).Text("");
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(4).Text("No items");
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(4).Text("");
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(4).Text("");
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(4).Text("");
                    }
                    else
                    {
                        foreach (var it in items.OrderBy(i => i.SrNo))
                        {
                            table.Cell().Border(1).BorderColor(Colors.Black).Padding(4).AlignCenter().Text(it.SrNo.ToString());
                            table.Cell().Border(1).BorderColor(Colors.Black).Padding(4).AlignCenter().Text(it.NoOfPersons.ToString());
                            table.Cell().Border(1).BorderColor(Colors.Black).Padding(4).Text(it.Particulars);
                            table.Cell().Border(1).BorderColor(Colors.Black).Padding(4).AlignRight().Text(it.TotalDuty.ToString("N0"));
                            table.Cell().Border(1).BorderColor(Colors.Black).Padding(4).AlignRight().Text(it.Rate.ToString("N2"));
                            table.Cell().Border(1).BorderColor(Colors.Black).Padding(4).AlignRight().Text(it.Amount.ToString("N2"));
                        }
                    }

                    // Total row
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(4).Text("TOTAL").Bold();
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(4).Text("");
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(4).Text("");
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(4).Text("");
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(4).Text("");
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(4).AlignRight().Text((bill?.TotalAmount ?? 0m).ToString("N2")).Bold();
                });
            });

            page.Footer().AlignCenter().Text(t => { t.Span("Page "); t.CurrentPageNumber(); t.Span(" of "); t.TotalPages(); });
        });
    }

    private void AddAttendancePage(IDocumentContainer container, IReadOnlyList<AgencyMonthlyExcelService.AttendanceDayRow> rows)
    {
        var daysInMonth = DateTime.DaysInMonth(_year, _month);
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(1f, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(8));

            page.Header().Column(col =>
            {
                col.Item().AlignCenter().Text(_header?.AgencyName ?? "Agency").Bold().FontSize(12);
                col.Item().Row(r =>
                {
                    r.RelativeItem().Text("LOCATION: " + _locationName);
                    r.RelativeItem().AlignRight().Text("ATTENDANCE FOR THE MONTH OF " + _monthLabel.ToUpperInvariant().Replace("-", " -")).Bold();
                });
            });

            page.Content().Table(table =>
            {
                table.ColumnsDefinition(def =>
                {
                    def.ConstantColumn(24);
                    def.RelativeColumn(2);
                    def.ConstantColumn(28);
                    for (int i = 0; i < Math.Min(31, daysInMonth); i++)
                        def.ConstantColumn(18);
                    def.ConstantColumn(28);
                });
                table.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten2).Padding(3).Text("S No.").Bold();
                table.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten2).Padding(3).Text("NAME").Bold();
                table.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten2).Padding(3).Text("TYPE").Bold();
                for (int d = 1; d <= Math.Min(31, daysInMonth); d++)
                    table.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten2).Padding(2).AlignCenter().Text(d.ToString());
                table.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten2).Padding(2).AlignCenter().Text("Total").Bold();
                foreach (var r in rows)
                {
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).Text(r.SNo.ToString());
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).Text(r.GuardName);
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).Text(r.Designation);
                    for (int d = 1; d <= daysInMonth; d++)
                        table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignCenter().Text(r.DayStatus.TryGetValue(d, out var s) ? s : "");
                    var totalP = r.DayStatus.Count(kv => string.Equals(kv.Value, "P", StringComparison.OrdinalIgnoreCase));
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignCenter().Text(totalP.ToString());
                }
            });

            page.Footer().AlignCenter().Text(t => { t.Span("Page "); t.CurrentPageNumber(); t.Span(" of "); t.TotalPages(); });
        });
    }

    private void AddWagesPage(IDocumentContainer container, IReadOnlyList<AgencyMonthlyExcelService.WageSheetRow> rows)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(1f, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(8));

            page.Header().Column(col =>
            {
                col.Item().AlignCenter().Text((_header?.AgencyName ?? "Agency").ToUpperInvariant()).Bold().FontSize(12);
                col.Item().Row(r =>
                {
                    r.RelativeItem().Text("Unit: " + _locationName);
                    r.RelativeItem().AlignRight().Text("Wages Sheet for the Month of " + _monthLabel.ToUpperInvariant().Replace("-", " -")).Bold();
                });
            });

            var headers = new[] { "S No.", "Name", "Designation", "UAN", "Basic/Day", "Att.", "OT Hrs", "ED", "Earn Wages", "Total Ded.", "OT Pay", "ED Pay", "Total Salary" };
            page.Content().Table(table =>
            {
                table.ColumnsDefinition(def =>
                {
                    for (int i = 0; i < headers.Length; i++)
                        def.RelativeColumn();
                });
                foreach (var h in headers)
                    table.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten2).Padding(3).Text(h).Bold();
                foreach (var r in rows)
                {
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).Text(r.SNo.ToString());
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).Text(r.Name);
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).Text(r.Designation);
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).Text(r.UAN ?? "");
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(r.BasicPerDay.ToString("N2"));
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignCenter().Text(r.Attendance.ToString());
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(r.OTHrs.ToString("N1"));
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(r.ED.ToString("N2"));
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(r.EarnWages.ToString("N2"));
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(r.TotalDeduction.ToString("N2"));
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(r.OTPayment.ToString("N2"));
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(r.EDPayment.ToString("N2"));
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2).AlignRight().Text(r.TotalSalary.ToString("N2"));
                }
            });

            page.Footer().AlignCenter().Text(t => { t.Span("Page "); t.CurrentPageNumber(); t.Span(" of "); t.TotalPages(); });
        });
    }
}
