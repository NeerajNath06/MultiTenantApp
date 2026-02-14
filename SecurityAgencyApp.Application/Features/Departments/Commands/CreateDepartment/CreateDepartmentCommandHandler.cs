using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Departments.Commands.CreateDepartment;

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateDepartmentCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        // Check if code already exists
        var deptRepo = _unitOfWork.Repository<Department>();
        var existing = await deptRepo.FirstOrDefaultAsync(
            d => d.Code == request.Code && d.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        if (existing != null)
        {
            return ApiResponse<Guid>.ErrorResponse("Department code already exists");
        }

        var department = new Department
        {
            TenantId = _tenantContext.TenantId.Value,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            IsActive = true
        };

        await deptRepo.AddAsync(department, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(department.Id, "Department created successfully");
    }
}
