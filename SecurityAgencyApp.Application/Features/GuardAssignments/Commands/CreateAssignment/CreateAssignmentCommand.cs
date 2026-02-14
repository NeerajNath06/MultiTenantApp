using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.GuardAssignments.Commands.CreateAssignment;

public class CreateAssignmentCommand : IRequest<ApiResponse<Guid>>
{
    public Guid GuardId { get; set; }
    public Guid SiteId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid? SupervisorId { get; set; }
    public DateTime AssignmentStartDate { get; set; }
    public DateTime? AssignmentEndDate { get; set; }
    public string? Remarks { get; set; }
}
