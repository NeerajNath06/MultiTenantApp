using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Sites.Queries.GetSupervisorsBySite;

/// <summary>
/// Returns only supervisors assigned to this site (SiteSupervisors created when site was created/updated).
/// Used by Assign Guard form: when user selects a site, supervisor dropdown shows only these supervisors.
/// </summary>
public class GetSupervisorsBySiteQuery : IRequest<ApiResponse<GetSupervisorsBySiteResponse>>
{
    public Guid SiteId { get; set; }
}

public class GetSupervisorsBySiteResponse
{
    public List<SupervisorItemDto> Items { get; set; } = new();
}

public class SupervisorItemDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    /// <summary>Display name: "FirstName LastName" or UserName if name empty.</summary>
    public string DisplayName { get; set; } = string.Empty;
}
