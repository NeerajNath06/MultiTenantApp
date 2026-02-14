using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Domain.Enums;

namespace SecurityAgencyApp.Application.Features.Incidents.Commands.CreateIncident;

public class CreateIncidentCommand : IRequest<ApiResponse<Guid>>
{
    public Guid SiteId { get; set; }
    public Guid? GuardId { get; set; }
    public DateTime IncidentDate { get; set; }
    public string IncidentType { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ActionTaken { get; set; }
}
