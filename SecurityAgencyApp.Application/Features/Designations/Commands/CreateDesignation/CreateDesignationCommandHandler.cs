using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Designations.Commands.CreateDesignation;

public class CreateDesignationCommandHandler : IRequestHandler<CreateDesignationCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateDesignationCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateDesignationCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        // Check if code already exists
        var designationRepo = _unitOfWork.Repository<Designation>();
        var existing = await designationRepo.FirstOrDefaultAsync(
            d => d.Code == request.Code && d.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        if (existing != null)
        {
            return ApiResponse<Guid>.ErrorResponse("Designation code already exists");
        }

        // Validate department if provided
        if (request.DepartmentId.HasValue)
        {
            var department = await _unitOfWork.Repository<Department>().GetByIdAsync(request.DepartmentId.Value, cancellationToken);
            if (department == null || department.TenantId != _tenantContext.TenantId.Value)
            {
                return ApiResponse<Guid>.ErrorResponse("Invalid department");
            }
        }

        var designation = new Designation
        {
            TenantId = _tenantContext.TenantId.Value,
            Name = request.Name,
            Code = request.Code,
            DepartmentId = request.DepartmentId,
            Description = request.Description,
            IsActive = true
        };

        await designationRepo.AddAsync(designation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(designation.Id, "Designation created successfully");
    }
}
