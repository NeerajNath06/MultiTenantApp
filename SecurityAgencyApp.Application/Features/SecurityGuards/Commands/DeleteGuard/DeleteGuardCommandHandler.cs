using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.SecurityGuards.Commands.DeleteGuard;

public class DeleteGuardCommandHandler : IRequestHandler<DeleteGuardCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteGuardCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteGuardCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        var guardRepo = _unitOfWork.Repository<SecurityGuard>();
        var guard = await guardRepo.GetByIdAsync(request.Id, cancellationToken);

        if (guard == null || guard.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("Security guard not found");
        }

        // Check if guard has active assignments
        var activeAssignments = await _unitOfWork.Repository<GuardAssignment>().CountAsync(
            ga => ga.GuardId == guard.Id && ga.Status == Domain.Enums.AssignmentStatus.Active,
            cancellationToken);

        if (activeAssignments > 0)
        {
            return ApiResponse<bool>.ErrorResponse("Cannot delete guard with active assignments. Please end assignments first.");
        }

        // Soft delete - mark as inactive
        guard.IsActive = false;
        guard.ModifiedDate = DateTime.UtcNow;
        await guardRepo.UpdateAsync(guard, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Security guard deleted successfully");
    }
}
