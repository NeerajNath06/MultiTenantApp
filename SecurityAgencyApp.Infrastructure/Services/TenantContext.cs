using SecurityAgencyApp.Application.Interfaces;

namespace SecurityAgencyApp.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public bool IsAuthenticated => TenantId.HasValue;
}
