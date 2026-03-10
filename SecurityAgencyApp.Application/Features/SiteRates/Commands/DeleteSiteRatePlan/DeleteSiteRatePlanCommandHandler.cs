using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.SiteRates.Commands.DeleteSiteRatePlan;

public class DeleteSiteRatePlanCommandHandler : IRequestHandler<DeleteSiteRatePlanCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteSiteRatePlanCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteSiteRatePlanCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");

        var repo = _unitOfWork.Repository<SiteRatePlan>();
        var plan = await repo.GetByIdAsync(request.Id, cancellationToken);
        if (plan == null || plan.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<bool>.ErrorResponse("Rate plan not found");

        plan.IsActive = false;
        plan.ModifiedDate = DateTime.UtcNow;
        await repo.UpdateAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Rate plan deleted");
    }
}
