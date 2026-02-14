using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.PatrolScans.Commands.CreatePatrolScan;

public class CreatePatrolScanCommand : IRequest<ApiResponse<Guid>>
{
    public Guid GuardId { get; set; }
    public Guid SiteId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string? CheckpointCode { get; set; }
}
