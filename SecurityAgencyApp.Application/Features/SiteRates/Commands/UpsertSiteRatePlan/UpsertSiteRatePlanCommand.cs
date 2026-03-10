using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.SiteRates.Commands.UpsertSiteRatePlan;

public class UpsertSiteRatePlanCommand : IRequest<ApiResponse<Guid>>
{
    /// <summary>If set, update this existing rate plan; otherwise create new.</summary>
    public Guid? Id { get; set; }
    public Guid SiteId { get; set; }
    public Guid ClientId { get; set; }
    public decimal RateAmount { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    /// <summary>EPF % for wages (e.g. 12).</summary>
    public decimal? EpfPercent { get; set; }
    /// <summary>ESIC % for wages (e.g. 0.75).</summary>
    public decimal? EsicPercent { get; set; }
    /// <summary>Allowance % for wages.</summary>
    public decimal? AllowancePercent { get; set; }
    /// <summary>EPF wage cap (₹). e.g. 15000 — EPF calculated on min(Earn Wages, this).</summary>
    public decimal? EpfWageCap { get; set; }
}

