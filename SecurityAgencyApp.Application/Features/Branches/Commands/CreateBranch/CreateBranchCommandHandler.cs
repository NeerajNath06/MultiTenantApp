using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Branches.Commands.CreateBranch;

public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateBranchCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");

        var repo = _unitOfWork.Repository<Branch>();
        var existing = await repo.FirstOrDefaultAsync(
            b => b.TenantId == _tenantContext.TenantId.Value && b.BranchCode == request.BranchCode,
            cancellationToken);
        if (existing != null)
            return ApiResponse<Guid>.ErrorResponse("Branch code already exists");

        var branch = new Branch
        {
            TenantId = _tenantContext.TenantId.Value,
            BranchCode = request.BranchCode.Trim(),
            BranchName = request.BranchName.Trim(),
            Address = request.Address.Trim(),
            City = request.City.Trim(),
            State = request.State.Trim(),
            PinCode = request.PinCode.Trim(),
            ContactPerson = Normalize(request.ContactPerson),
            ContactPhone = Normalize(request.ContactPhone),
            ContactEmail = Normalize(request.ContactEmail),
            GstNumber = Normalize(request.GstNumber),
            LabourLicenseNumber = Normalize(request.LabourLicenseNumber),
            NumberPrefix = Normalize(request.NumberPrefix),
            IsHeadOffice = request.IsHeadOffice,
            IsActive = request.IsActive
        };

        await repo.AddAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<Guid>.SuccessResponse(branch.Id, "Branch created successfully");
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
