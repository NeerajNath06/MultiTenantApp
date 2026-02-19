using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SecurityAgencyApp.API.Services;

/// <summary>Enterprise export: Excel and PDF with print-ready layout (title, date, table, page numbers, confidential footer).</summary>
public static class ExportHelper
{
    /// <summary>Generate Excel (.xlsx) with report title and generated date in header.</summary>
    public static byte[] ToExcel<T>(string sheetName, string reportTitle, IEnumerable<T> data) where T : class
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(sheetName);
        int row = 1;
        ws.Cell(row, 1).Value = reportTitle;
        ws.Range(row, 1, row, 10).Merge().Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 14;
        row++;
        ws.Cell(row, 1).Value = "Generated: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm UTC");
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

    /// <summary>Generate PDF with enterprise layout: header (title + date), table, footer (page x of y, confidential).</summary>
    public static byte[] ToPdf(string reportTitle, string[] columnHeaders, IEnumerable<string[]> rows)
    {
        var rowList = rows.ToList();
        if (rowList.Count == 0)
            rowList.Add(columnHeaders.Select(_ => "No data").ToArray());
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
                        col.Item().Text(reportTitle).Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
                        col.Item().Text("Generated: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm UTC")).FontSize(8).FontColor(Colors.Grey.Medium);
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
