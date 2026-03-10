using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.SiteRates.Queries.GetSiteRateHistory;

public class GetSiteRateHistoryQuery : IRequest<ApiResponse<List<SiteRateHistoryDto>>>
{
    public Guid SiteId { get; set; }
    public bool IncludeInactive { get; set; } = true;
}

public class SiteRateHistoryDto
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public Guid ClientId { get; set; }
    public decimal RateAmount { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public decimal? EpfPercent { get; set; }
    public decimal? EsicPercent { get; set; }
    public decimal? AllowancePercent { get; set; }
    public decimal? EpfWageCap { get; set; }
}

