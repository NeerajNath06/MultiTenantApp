using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Dashboard.Queries.GetMonthlySiteSummary;

public class GetMonthlySiteSummaryQuery : IRequest<ApiResponse<MonthlySiteSummaryResponseDto>>
{
    public int Year { get; set; }
    public int Month { get; set; } // 1-12
}

public class MonthlySiteSummaryResponseDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<MonthlySiteSummaryItemDto> Items { get; set; } = new();
}

public class MonthlySiteSummaryItemDto
{
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public int PresentDaysTotal { get; set; }
    public decimal? LatestBillTotal { get; set; }
    public decimal? LatestWageTotal { get; set; }
}

