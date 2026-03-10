using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.SiteRates.Queries.GetSiteRateHistory;

public class GetSiteRateHistoryQueryHandler : IRequestHandler<GetSiteRateHistoryQuery, ApiResponse<List<SiteRateHistoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetSiteRateHistoryQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<List<SiteRateHistoryDto>>> Handle(GetSiteRateHistoryQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<List<SiteRateHistoryDto>>.ErrorResponse("Tenant context not found");

        var repo = _unitOfWork.Repository<SiteRatePlan>();
        var list = await repo.FindAsync(r => r.SiteId == request.SiteId && (request.IncludeInactive || r.IsActive), cancellationToken);
        var items = list
            .OrderByDescending(r => r.EffectiveFrom)
            .ThenByDescending(r => r.CreatedDate)
            .Select(r => new SiteRateHistoryDto
            {
                Id = r.Id,
                SiteId = r.SiteId,
                ClientId = r.ClientId,
                RateAmount = r.RateAmount,
                EffectiveFrom = r.EffectiveFrom,
                EffectiveTo = r.EffectiveTo,
                IsActive = r.IsActive,
                EpfPercent = r.EpfPercent,
                EsicPercent = r.EsicPercent,
                AllowancePercent = r.AllowancePercent,
                EpfWageCap = r.EpfWageCap
            }).ToList();

        return ApiResponse<List<SiteRateHistoryDto>>.SuccessResponse(items);
    }
}

