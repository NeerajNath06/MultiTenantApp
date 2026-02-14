using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.GuardAssignments.Commands.UpdateAssignment;

public class UpdateAssignmentCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
    public Guid GuardId { get; set; }
    public Guid SiteId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid? SupervisorId { get; set; }
    public DateTime AssignmentStartDate { get; set; }
    public DateTime? AssignmentEndDate { get; set; }
    public string? Remarks { get; set; }
}
