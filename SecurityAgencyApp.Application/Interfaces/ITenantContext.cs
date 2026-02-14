namespace SecurityAgencyApp.Application.Interfaces;

public interface ITenantContext
{
    Guid? TenantId { get; }
    string? TenantName { get; }
    bool IsAuthenticated { get; }
}
