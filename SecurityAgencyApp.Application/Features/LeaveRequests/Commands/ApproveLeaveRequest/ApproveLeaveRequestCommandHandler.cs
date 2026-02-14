using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.LeaveRequests.Commands.ApproveLeaveRequest;

public class ApproveLeaveRequestCommandHandler : IRequestHandler<ApproveLeaveRequestCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public ApproveLeaveRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<bool>> Handle(ApproveLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant or user context not found");
        }

        var leaveRequest = await _unitOfWork.Repository<LeaveRequest>().GetByIdAsync(request.LeaveRequestId, cancellationToken);
        if (leaveRequest == null || leaveRequest.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("Leave request not found");
        }

        if (leaveRequest.Status != "Pending")
        {
            return ApiResponse<bool>.ErrorResponse("Leave request is not in pending status");
        }

        leaveRequest.Status = request.IsApproved ? "Approved" : "Rejected";
        leaveRequest.ApprovedBy = _currentUserService.UserId.Value;
        leaveRequest.ApprovedDate = DateTime.UtcNow;
        leaveRequest.RejectionReason = request.IsApproved ? null : request.RejectionReason;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, 
            request.IsApproved ? "Leave request approved successfully" : "Leave request rejected");
    }
}
