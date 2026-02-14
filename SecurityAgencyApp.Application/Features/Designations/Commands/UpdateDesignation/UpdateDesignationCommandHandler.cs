using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Designations.Commands.UpdateDesignation;

public class UpdateDesignationCommandHandler : IRequestHandler<UpdateDesignationCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateDesignationCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateDesignationCommand request, CancellationToken cancellationToken)
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

        // Check if code already exists (excluding current designation)
        var existing = await designationRepo.FirstOrDefaultAsync(
            d => d.Code == request.Code && 
                 d.Id != request.Id && 
                 d.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        if (existing != null)
        {
            return ApiResponse<bool>.ErrorResponse("Designation code already exists");
        }

        // Validate department if provided
        if (request.DepartmentId.HasValue)
        {
            var department = await _unitOfWork.Repository<Department>().GetByIdAsync(request.DepartmentId.Value, cancellationToken);
            if (department == null || department.TenantId != _tenantContext.TenantId.Value)
            {
                return ApiResponse<bool>.ErrorResponse("Invalid department");
            }
        }

        designation.Name = request.Name;
        designation.Code = request.Code;
        designation.DepartmentId = request.DepartmentId;
        designation.Description = request.Description;
        designation.IsActive = request.IsActive;
        designation.ModifiedDate = DateTime.UtcNow;

        await designationRepo.UpdateAsync(designation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Designation updated successfully");
    }
}
