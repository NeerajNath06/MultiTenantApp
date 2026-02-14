using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Departments.Commands.UpdateDepartment;

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateDepartmentCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
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

        // Check if code is already taken by another department
        if (department.Code != request.Code)
        {
            var existing = await deptRepo.FirstOrDefaultAsync(
                d => d.Code == request.Code && d.Id != request.Id && d.TenantId == _tenantContext.TenantId.Value,
                cancellationToken);

            if (existing != null)
            {
                return ApiResponse<bool>.ErrorResponse("Department code already exists");
            }
        }

        department.Name = request.Name;
        department.Code = request.Code;
        department.Description = request.Description;
        department.IsActive = request.IsActive;
        department.ModifiedDate = DateTime.UtcNow;

        await deptRepo.UpdateAsync(department, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Department updated successfully");
    }
}
