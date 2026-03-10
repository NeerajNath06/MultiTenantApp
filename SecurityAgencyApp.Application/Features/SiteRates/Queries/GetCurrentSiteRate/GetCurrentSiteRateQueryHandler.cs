using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.SiteRates.Queries.GetCurrentSiteRate;

public class GetCurrentSiteRateQueryHandler : IRequestHandler<GetCurrentSiteRateQuery, ApiResponse<SiteRateDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetCurrentSiteRateQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<SiteRateDto>> Handle(GetCurrentSiteRateQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<SiteRateDto>.ErrorResponse("Tenant context not found");

        var asOf = (request.AsOfDate ?? DateTime.UtcNow).Date;
        var repo = _unitOfWork.Repository<SiteRatePlan>();
        var candidates = await repo.FindAsync(r =>
            r.SiteId == request.SiteId &&
            r.IsActive &&
            r.EffectiveFrom.Date <= asOf &&
            (r.EffectiveTo == null || r.EffectiveTo.Value.Date >= asOf), cancellationToken);

        var current = candidates.OrderByDescending(c => c.EffectiveFrom).FirstOrDefault();
        if (current == null)
            return ApiResponse<SiteRateDto>.ErrorResponse("Rate plan not found for this site");

        string? clientName = null;
        if (current.ClientId != Guid.Empty)
        {
            var client = await _unitOfWork.Repository<Client>().GetByIdAsync(current.ClientId, cancellationToken);
            clientName = client?.CompanyName;
        }

        return ApiResponse<SiteRateDto>.SuccessResponse(new SiteRateDto
        {
            SiteId = current.SiteId,
            ClientId = current.ClientId,
            ClientName = clientName,
            RateAmount = current.RateAmount,
            EffectiveFrom = current.EffectiveFrom,
            EffectiveTo = current.EffectiveTo,
            EpfPercent = current.EpfPercent,
            EsicPercent = current.EsicPercent,
            AllowancePercent = current.AllowancePercent,
            EpfWageCap = current.EpfWageCap
        });
    }
}

