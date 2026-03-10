using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Incidents.Queries.GetIncidentById;

public class GetIncidentByIdQuery : IRequest<ApiResponse<IncidentDetailDto>>
{
    public Guid Id { get; set; }
}

public class IncidentDetailDto
{
    public Guid Id { get; set; }
    public string IncidentNumber { get; set; } = string.Empty;
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public Guid? GuardId { get; set; }
    public string? GuardName { get; set; }
    public DateTime IncidentDate { get; set; }
    public string IncidentType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ActionTaken { get; set; }
}
