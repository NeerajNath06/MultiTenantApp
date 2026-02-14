using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.LeaveRequests.Commands.ApproveLeaveRequest;

public class ApproveLeaveRequestCommand : IRequest<ApiResponse<bool>>
{
    public Guid LeaveRequestId { get; set; }
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
}
