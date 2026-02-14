using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Designations.Queries.GetDesignationById;

public class GetDesignationByIdQueryHandler : IRequestHandler<GetDesignationByIdQuery, ApiResponse<DesignationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetDesignationByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<DesignationDto>> Handle(GetDesignationByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<DesignationDto>.ErrorResponse("Tenant context not found");
        }

        var designationRepo = _unitOfWork.Repository<Designation>();
        var designation = await designationRepo.GetByIdAsync(request.Id, cancellationToken);

        if (designation == null || designation.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<DesignationDto>.ErrorResponse("Designation not found");
        }

        Department? department = null;
        if (designation.DepartmentId.HasValue)
        {
            department = await _unitOfWork.Repository<Department>().GetByIdAsync(designation.DepartmentId.Value, cancellationToken);
        }

        var designationDto = new DesignationDto
        {
            Id = designation.Id,
            Name = designation.Name,
            Code = designation.Code,
            DepartmentId = designation.DepartmentId,
            DepartmentName = department?.Name,
            Description = designation.Description,
            IsActive = designation.IsActive,
            CreatedDate = designation.CreatedDate,
            ModifiedDate = designation.ModifiedDate
        };

        return ApiResponse<DesignationDto>.SuccessResponse(designationDto, "Designation retrieved successfully");
    }
}
