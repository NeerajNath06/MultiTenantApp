using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Shifts.Commands.DeleteShift;

public class DeleteShiftCommandHandler : IRequestHandler<DeleteShiftCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteShiftCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteShiftCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        var shiftRepo = _unitOfWork.Repository<Shift>();
        var shift = await shiftRepo.GetByIdAsync(request.Id, cancellationToken);

        if (shift == null || shift.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("Shift not found");
        }

        // Check if shift is used in any active assignments
        var activeAssignments = await _unitOfWork.Repository<GuardAssignment>().CountAsync(
            ga => ga.ShiftId == shift.Id && ga.Status == Domain.Enums.AssignmentStatus.Active,
            cancellationToken);

        if (activeAssignments > 0)
        {
            return ApiResponse<bool>.ErrorResponse("Cannot delete shift that is used in active assignments. Please end assignments first.");
        }

        // Soft delete
        shift.IsActive = false;
        shift.ModifiedDate = DateTime.UtcNow;
        await shiftRepo.UpdateAsync(shift, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Shift deleted successfully");
    }
}
