using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SecurityAgencyApp.API.Services;

/// <summary>Enterprise report header: agency name, address, report title and subtitle (e.g. date range).</summary>
public class ExportReportHeader
{
    public string AgencyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string ReportTitle { get; set; } = string.Empty;
    public string? ReportSubTitle { get; set; }
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;

    public string FormattedAddress => string.Join(", ", new[] { Address?.Trim(), City, State, PinCode }.Where(s => !string.IsNullOrWhiteSpace(s)));
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PinCode { get; set; }
}

/// <summary>Enterprise export: Excel and PDF with agency header, print-ready layout, page numbers, confidential footer.</summary>
public static class ExportHelper
{
    /// <summary>Generate Excel with enterprise header (agency name, address, report title, subtitle, generated date) then data table.</summary>
    public static byte[] ToExcel<T>(string sheetName, string reportTitle, IEnumerable<T> data, ExportReportHeader? header = null) where T : class
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(sheetName);
        int row = 1;
        if (header != null)
        {
            ws.Cell(row, 1).Value = header.AgencyName;
            ws.Range(row, 1, row, 10).Merge().Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 16;
            row++;
            if (!string.IsNullOrWhiteSpace(header.FormattedAddress))
            {
                ws.Cell(row, 1).Value = header.FormattedAddress;
                ws.Range(row, 1, row, 10).Merge();
                row++;
            }
            if (!string.IsNullOrWhiteSpace(header.Phone)) { ws.Cell(row, 1).Value = "Ph: " + header.Phone; ws.Range(row, 1, row, 5).Merge(); row++; }
            if (!string.IsNullOrWhiteSpace(header.Email)) { ws.Cell(row, 1).Value = "Email: " + header.Email; ws.Range(row, 1, row, 5).Merge(); row++; }
            row++;
        }
        ws.Cell(row, 1).Value = reportTitle;
        ws.Range(row, 1, row, 10).Merge().Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 14;
        row++;
        if (header != null && !string.IsNullOrWhiteSpace(header.ReportSubTitle))
        {
            ws.Cell(row, 1).Value = header.ReportSubTitle;
            ws.Range(row, 1, row, 10).Merge();
            row++;
        }
        ws.Cell(row, 1).Value = "Generated: " + (header?.GeneratedAtUtc ?? DateTime.UtcNow).ToString("yyyy-MM-dd HH:mm UTC");
        ws.Cell(row, 1).Style.Font.Italic = true;
        row += 2;
        var list = data.ToList();
        if (list.Count > 0)
            ws.Cell(row, 1).InsertTable(list, tableName: "Data", createTable: true);
        else
            ws.Cell(row, 1).Value = "No data";
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    /// <summary>Generate PDF with enterprise header (agency, address, title, subtitle), table, footer (page x of y, confidential).</summary>
    public static byte[] ToPdf(string reportTitle, string[] columnHeaders, IEnumerable<string[]> rows, ExportReportHeader? header = null)
    {
        var rowList = rows.ToList();
        if (rowList.Count == 0)
            rowList.Add(columnHeaders.Select(_ => "No data").ToArray());
        var generated = header?.GeneratedAtUtc ?? DateTime.UtcNow;
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header()
                    .Column(col =>
                    {
                        if (header != null)
                        {
                            col.Item().Text(header.AgencyName).Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
                            if (!string.IsNullOrWhiteSpace(header.FormattedAddress))
                                col.Item().Text(header.FormattedAddress).FontSize(8).FontColor(Colors.Grey.Medium);
                            if (!string.IsNullOrWhiteSpace(header.Phone))
                                col.Item().Text("Ph: " + header.Phone + (string.IsNullOrWhiteSpace(header.Email) ? "" : "  |  Email: " + header.Email)).FontSize(8).FontColor(Colors.Grey.Medium);
                            else if (!string.IsNullOrWhiteSpace(header.Email))
                                col.Item().Text("Email: " + header.Email).FontSize(8).FontColor(Colors.Grey.Medium);
                            col.Item().PaddingTop(4);
                        }
                        col.Item().Text(reportTitle).Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                        if (header != null && !string.IsNullOrWhiteSpace(header.ReportSubTitle))
                            col.Item().Text(header.ReportSubTitle).FontSize(8).FontColor(Colors.Grey.Medium);
                        col.Item().Text("Generated: " + generated.ToString("yyyy-MM-dd HH:mm UTC")).FontSize(8).FontColor(Colors.Grey.Medium);
                    });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    var cols = columnHeaders.Length;
                    table.ColumnsDefinition(def =>
                    {
                        for (int i = 0; i < cols; i++)
                            def.RelativeColumn();
                    });
                    foreach (var h in columnHeaders)
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(h).Bold();
                    foreach (var row in rowList)
                    {
                        foreach (var cell in row)
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(cell ?? "");
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .PaddingTop(8)
                    .DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium))
                    .Column(col =>
                    {
                        col.Item().Text(t =>
                        {
                            t.Span("Page ");
                            t.CurrentPageNumber();
                            t.Span(" of ");
                            t.TotalPages();
                        });
                        col.Item().Text("Confidential - For authorized use only.");
                    });
            });
        }).GeneratePdf();
    }
}
