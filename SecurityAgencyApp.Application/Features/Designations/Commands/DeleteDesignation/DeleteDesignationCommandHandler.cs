using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Designations.Commands.DeleteDesignation;

public class DeleteDesignationCommandHandler : IRequestHandler<DeleteDesignationCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteDesignationCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteDesignationCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        var designationRepo = _unitOfWork.Repository<Designation>();
        var designation = await designationRepo.GetByIdAsync(request.Id, cancellationToken);

        if (designation == null || designation.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("Designation not found");
        }

        // Check if designation is used by any users
        var userCount = await _unitOfWork.Repository<User>().CountAsync(
            u => u.DesignationId == designation.Id && u.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        if (userCount > 0)
        {
            return ApiResponse<bool>.ErrorResponse("Cannot delete designation that is assigned to users. Please reassign users first.");
        }

        // Soft delete
        designation.IsActive = false;
        designation.ModifiedDate = DateTime.UtcNow;
        await designationRepo.UpdateAsync(designation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Designation deleted successfully");
    }
}
