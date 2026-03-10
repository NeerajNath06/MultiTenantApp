using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.LeaveRequests.Queries.GetLeaveRequestById;

public class GetLeaveRequestByIdQueryHandler : IRequestHandler<GetLeaveRequestByIdQuery, ApiResponse<LeaveRequestDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetLeaveRequestByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<LeaveRequestDetailDto>> Handle(GetLeaveRequestByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<LeaveRequestDetailDto>.ErrorResponse("Tenant context not found");

        var leave = await _unitOfWork.Repository<LeaveRequest>().GetByIdAsync(request.Id, cancellationToken);
        if (leave == null || leave.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<LeaveRequestDetailDto>.ErrorResponse("Leave request not found");

        var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(leave.GuardId, cancellationToken);
        string? approvedByName = null;
        if (leave.ApprovedBy.HasValue)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(leave.ApprovedBy.Value, cancellationToken);
            approvedByName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : null;
            if (string.IsNullOrWhiteSpace(approvedByName)) approvedByName = user?.Email;
        }

        var dto = new LeaveRequestDetailDto
        {
            Id = leave.Id,
            GuardId = leave.GuardId,
            GuardName = guard != null ? $"{guard.FirstName} {guard.LastName}".Trim() : "",
            GuardCode = guard?.GuardCode ?? "",
            LeaveType = leave.LeaveType,
            StartDate = leave.StartDate,
            EndDate = leave.EndDate,
            TotalDays = leave.TotalDays,
            Reason = leave.Reason,
            Status = leave.Status,
            ApprovedBy = leave.ApprovedBy,
            ApprovedByName = approvedByName,
            ApprovedDate = leave.ApprovedDate,
            RejectionReason = leave.RejectionReason,
            Notes = leave.Notes,
            IsActive = leave.IsActive,
            CreatedDate = leave.CreatedDate
        };
        return ApiResponse<LeaveRequestDetailDto>.SuccessResponse(dto, "Leave request retrieved");
    }
}
