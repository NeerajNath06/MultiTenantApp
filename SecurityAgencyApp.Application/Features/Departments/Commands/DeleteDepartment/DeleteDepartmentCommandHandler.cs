using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Departments.Commands.DeleteDepartment;

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteDepartmentCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        var deptRepo = _unitOfWork.Repository<Department>();
        var department = await deptRepo.GetByIdAsync(request.Id, cancellationToken);

        if (department == null || department.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("Department not found");
        }

        // Check if department has users
        var userCount = await _unitOfWork.Repository<User>().CountAsync(
            u => u.DepartmentId == department.Id && u.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        if (userCount > 0)
        {
            return ApiResponse<bool>.ErrorResponse("Cannot delete department with existing users. Please reassign users first.");
        }

        // Soft delete - mark as inactive
        department.IsActive = false;
        department.ModifiedDate = DateTime.UtcNow;
        await deptRepo.UpdateAsync(department, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Department deleted successfully");
    }
}
