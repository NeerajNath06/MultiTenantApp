using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Branches.Commands.UpdateBranch;

public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateBranchCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");

        var repo = _unitOfWork.Repository<Branch>();
        var branch = await repo.GetByIdAsync(request.Id, cancellationToken);
        if (branch == null || branch.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<bool>.ErrorResponse("Branch not found");

        var existing = await repo.FirstOrDefaultAsync(
            b => b.TenantId == _tenantContext.TenantId.Value && b.BranchCode == request.BranchCode && b.Id != request.Id,
            cancellationToken);
        if (existing != null)
            return ApiResponse<bool>.ErrorResponse("Branch code already exists");

        branch.BranchCode = request.BranchCode.Trim();
        branch.BranchName = request.BranchName.Trim();
        branch.Address = request.Address.Trim();
        branch.City = request.City.Trim();
        branch.State = request.State.Trim();
        branch.PinCode = request.PinCode.Trim();
        branch.ContactPerson = Normalize(request.ContactPerson);
        branch.ContactPhone = Normalize(request.ContactPhone);
        branch.ContactEmail = Normalize(request.ContactEmail);
        branch.GstNumber = Normalize(request.GstNumber);
        branch.LabourLicenseNumber = Normalize(request.LabourLicenseNumber);
        branch.NumberPrefix = Normalize(request.NumberPrefix);
        branch.IsHeadOffice = request.IsHeadOffice;
        branch.IsActive = request.IsActive;
        branch.ModifiedDate = DateTime.UtcNow;

        await repo.UpdateAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
