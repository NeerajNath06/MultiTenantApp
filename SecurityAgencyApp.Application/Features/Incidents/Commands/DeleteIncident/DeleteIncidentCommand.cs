using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Incidents.Commands.DeleteIncident;

public class DeleteIncidentCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
