using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Branches.Queries.GetBranchById;

public class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, ApiResponse<BranchDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetBranchByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<BranchDto>> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<BranchDto>.ErrorResponse("Tenant context not found");

        var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(request.Id, cancellationToken);
        if (branch == null || branch.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<BranchDto>.ErrorResponse("Branch not found");

        return ApiResponse<BranchDto>.SuccessResponse(Map(branch), "Branch retrieved successfully");
    }

    private static BranchDto Map(Branch branch)
    {
        return new BranchDto
        {
            Id = branch.Id,
            BranchCode = branch.BranchCode,
            BranchName = branch.BranchName,
            Address = branch.Address,
            City = branch.City,
            State = branch.State,
            PinCode = branch.PinCode,
            ContactPerson = branch.ContactPerson,
            ContactPhone = branch.ContactPhone,
            ContactEmail = branch.ContactEmail,
            GstNumber = branch.GstNumber,
            LabourLicenseNumber = branch.LabourLicenseNumber,
            NumberPrefix = branch.NumberPrefix,
            IsHeadOffice = branch.IsHeadOffice,
            IsActive = branch.IsActive,
            CreatedDate = branch.CreatedDate,
            ModifiedDate = branch.ModifiedDate
        };
    }
}
