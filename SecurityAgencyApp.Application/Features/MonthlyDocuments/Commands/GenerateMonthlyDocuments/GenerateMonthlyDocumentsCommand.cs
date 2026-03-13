using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.MonthlyDocuments.Commands.GenerateMonthlyDocuments;

public class GenerateMonthlyDocumentsCommand : IRequest<ApiResponse<GenerateMonthlyDocumentsResultDto>>
{
    public Guid SiteId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
}

public class GenerateMonthlyDocumentsResultDto
{
    public Guid BillId { get; set; }
    public Guid WageId { get; set; }
    public int PresentAttendanceCount { get; set; }
    public decimal RateAmount { get; set; }
}

