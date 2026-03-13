using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Branches.Queries.GetBranchList;

public class GetBranchListQueryHandler : IRequestHandler<GetBranchListQuery, ApiResponse<BranchListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetBranchListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<BranchListResponseDto>> Handle(GetBranchListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<BranchListResponseDto>.ErrorResponse("Tenant context not found");

        var query = _unitOfWork.Repository<Branch>().GetQueryable()
            .Where(b => b.TenantId == _tenantContext.TenantId.Value && (request.IncludeInactive || b.IsActive));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(b =>
                b.BranchCode.ToLower().Contains(search) ||
                b.BranchName.ToLower().Contains(search) ||
                b.City.ToLower().Contains(search) ||
                b.State.ToLower().Contains(search));
        }

        var items = query.OrderBy(b => b.BranchName)
            .Select(b => new BranchDto
            {
                Id = b.Id,
                BranchCode = b.BranchCode,
                BranchName = b.BranchName,
                Address = b.Address,
                City = b.City,
                State = b.State,
                PinCode = b.PinCode,
                ContactPerson = b.ContactPerson,
                ContactPhone = b.ContactPhone,
                ContactEmail = b.ContactEmail,
                GstNumber = b.GstNumber,
                LabourLicenseNumber = b.LabourLicenseNumber,
                NumberPrefix = b.NumberPrefix,
                IsHeadOffice = b.IsHeadOffice,
                IsActive = b.IsActive,
                CreatedDate = b.CreatedDate,
                ModifiedDate = b.ModifiedDate
            })
            .ToList();

        return ApiResponse<BranchListResponseDto>.SuccessResponse(new BranchListResponseDto
        {
            Items = items,
            TotalCount = items.Count
        }, "Branches retrieved successfully");
    }
}
