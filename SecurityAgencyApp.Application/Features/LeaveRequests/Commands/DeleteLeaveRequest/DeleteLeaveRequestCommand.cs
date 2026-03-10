using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.LeaveRequests.Commands.DeleteLeaveRequest;

public class DeleteLeaveRequestCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
