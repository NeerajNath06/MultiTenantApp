using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class SalaryStructure : TenantEntity
{
    public string StructureName { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public Guid? SiteId { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public decimal BasicAmount { get; set; }
    public decimal HraAmount { get; set; }
    public decimal WashingAllowance { get; set; }
    public decimal ConveyanceAllowance { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal OtherAllowanceAmount { get; set; }
    public decimal EpfPercent { get; set; }
    public decimal EsicPercent { get; set; }
    public decimal ProfessionalTaxAmount { get; set; }
    public decimal TdsAmount { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual Branch? Branch { get; set; }
    public virtual Site? Site { get; set; }
}
