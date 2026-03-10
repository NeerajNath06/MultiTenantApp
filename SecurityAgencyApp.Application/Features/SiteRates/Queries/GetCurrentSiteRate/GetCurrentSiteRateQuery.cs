using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.SiteRates.Queries.GetCurrentSiteRate;

public class GetCurrentSiteRateQuery : IRequest<ApiResponse<SiteRateDto>>
{
    public Guid SiteId { get; set; }
    public DateTime? AsOfDate { get; set; } // defaults to today
}

public class SiteRateDto
{
    public Guid SiteId { get; set; }
    public Guid ClientId { get; set; }
    public string? ClientName { get; set; }
    public decimal RateAmount { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public decimal? EpfPercent { get; set; }
    public decimal? EsicPercent { get; set; }
    public decimal? AllowancePercent { get; set; }
    public decimal? EpfWageCap { get; set; }
}

