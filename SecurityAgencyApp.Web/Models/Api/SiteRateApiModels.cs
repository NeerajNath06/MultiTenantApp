namespace SecurityAgencyApp.Web.Models.Api;

public class SiteRateDto
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public decimal RateAmount { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public decimal? EpfPercent { get; set; }
    public decimal? EsicPercent { get; set; }
    public decimal? AllowancePercent { get; set; }
    public decimal? EpfWageCap { get; set; }
    public bool IsActive { get; set; }
}

public class SiteRateHistoryDto : SiteRateDto
{
}

public class UpsertSiteRateRequest
{
    /// <summary>Set to update existing plan; omit to create new.</summary>
    public Guid? Id { get; set; }
    public Guid SiteId { get; set; }
    public Guid ClientId { get; set; }
    public decimal RateAmount { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public decimal? EpfPercent { get; set; }
    public decimal? EsicPercent { get; set; }
    public decimal? AllowancePercent { get; set; }
    public decimal? EpfWageCap { get; set; }
}

