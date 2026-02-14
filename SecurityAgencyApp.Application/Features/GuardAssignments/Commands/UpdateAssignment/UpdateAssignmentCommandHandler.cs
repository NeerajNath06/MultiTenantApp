using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.GuardAssignments.Commands.UpdateAssignment;

public class UpdateAssignmentCommandHandler : IRequestHandler<UpdateAssignmentCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateAssignmentCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateAssignmentCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");

        var repo = _unitOfWork.Repository<GuardAssignment>();
        var assignment = await repo.GetByIdAsync(request.Id, cancellationToken);
        if (assignment == null || assignment.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<bool>.ErrorResponse("Assignment not found");

        var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(request.GuardId, cancellationToken);
        if (guard == null || guard.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<bool>.ErrorResponse("Guard not found");

        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(request.SiteId, cancellationToken);
        if (site == null || site.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<bool>.ErrorResponse("Site not found");

        var shift = await _unitOfWork.Repository<Shift>().GetByIdAsync(request.ShiftId, cancellationToken);
        if (shift == null || shift.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<bool>.ErrorResponse("Shift not found");

        assignment.GuardId = request.GuardId;
        assignment.SiteId = request.SiteId;
        assignment.ShiftId = request.ShiftId;
        assignment.SupervisorId = request.SupervisorId;
        assignment.AssignmentStartDate = request.AssignmentStartDate;
        assignment.AssignmentEndDate = request.AssignmentEndDate;
        assignment.Remarks = request.Remarks;

        await repo.UpdateAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Assignment updated successfully");
    }
}
