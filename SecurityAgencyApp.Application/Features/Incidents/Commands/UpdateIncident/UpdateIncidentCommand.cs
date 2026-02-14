using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Domain.Enums;

namespace SecurityAgencyApp.Application.Features.Incidents.Commands.UpdateIncident;

public class UpdateIncidentCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
    public string? ActionTaken { get; set; }
    public IncidentStatus Status { get; set; }
}
