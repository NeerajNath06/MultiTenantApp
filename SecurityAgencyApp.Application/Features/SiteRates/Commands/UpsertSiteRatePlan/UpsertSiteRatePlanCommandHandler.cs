using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.SiteRates.Commands.UpsertSiteRatePlan;

public class UpsertSiteRatePlanCommandHandler : IRequestHandler<UpsertSiteRatePlanCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public UpsertSiteRatePlanCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<Guid>> Handle(UpsertSiteRatePlanCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        if (!_currentUserService.UserId.HasValue)
            return ApiResponse<Guid>.ErrorResponse("User context required to save rate plan");
        if (request.RateAmount <= 0)
            return ApiResponse<Guid>.ErrorResponse("Rate must be greater than 0");
        if (request.EffectiveTo.HasValue && request.EffectiveTo.Value.Date < request.EffectiveFrom.Date)
            return ApiResponse<Guid>.ErrorResponse("EffectiveTo cannot be before EffectiveFrom");

        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(request.SiteId, cancellationToken);
        if (site == null)
            return ApiResponse<Guid>.ErrorResponse("Site not found");

        var clientId = request.ClientId != Guid.Empty
            ? request.ClientId
            : (site.ClientId ?? Guid.Empty);

        if (clientId == Guid.Empty && !string.IsNullOrWhiteSpace(site.ClientName))
        {
            var clientRepo = _unitOfWork.Repository<Client>();
            var clientByName = (await clientRepo.FindAsync(c =>
                c.TenantId == _tenantContext.TenantId.Value &&
                c.CompanyName != null && c.CompanyName.Trim().ToLower() == site.ClientName.Trim().ToLower(),
                cancellationToken)).FirstOrDefault();
            if (clientByName != null)
                clientId = clientByName.Id;
        }

        if (clientId == Guid.Empty)
            return ApiResponse<Guid>.ErrorResponse("ClientId is required for rate plan. Link this site to a client (Edit Site → set Client), or create a client whose name matches the site's client name.");

        var repo = _unitOfWork.Repository<SiteRatePlan>();

        // Update existing plan if Id provided
        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existingPlan = await repo.GetByIdAsync(request.Id.Value, cancellationToken);
            if (existingPlan != null && existingPlan.SiteId == request.SiteId)
            {
                existingPlan.ClientId = clientId;
                existingPlan.RateAmount = request.RateAmount;
                existingPlan.EffectiveFrom = request.EffectiveFrom.Date;
                existingPlan.EffectiveTo = request.EffectiveTo?.Date;
                existingPlan.IsActive = true;
                existingPlan.EpfPercent = request.EpfPercent;
                existingPlan.EsicPercent = request.EsicPercent;
                existingPlan.AllowancePercent = request.AllowancePercent;
                existingPlan.EpfWageCap = request.EpfWageCap;
                existingPlan.ModifiedDate = DateTime.UtcNow;
                existingPlan.ModifiedBy = _currentUserService.UserId;
                await repo.UpdateAsync(existingPlan, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return ApiResponse<Guid>.SuccessResponse(existingPlan.Id, "Rate plan updated");
            }
        }

        // Close any active rate plan overlapping this effective-from date (only when creating new).
        var existing = await repo.FindAsync(r =>
                r.SiteId == request.SiteId &&
                r.IsActive &&
                (r.EffectiveTo == null || r.EffectiveTo.Value.Date >= request.EffectiveFrom.Date),
            cancellationToken);

        foreach (var e in existing)
        {
            // If prior plan starts before new plan, close it one day before.
            if (e.EffectiveFrom.Date < request.EffectiveFrom.Date)
            {
                e.EffectiveTo = request.EffectiveFrom.Date.AddDays(-1);
                e.IsActive = e.EffectiveTo >= e.EffectiveFrom;
                await repo.UpdateAsync(e, cancellationToken);
            }
            else
            {
                // Same-day or future plan: deactivate (keeps history).
                e.IsActive = false;
                await repo.UpdateAsync(e, cancellationToken);
            }
        }

        var entity = new SiteRatePlan
        {
            TenantId = _tenantContext.TenantId.Value,
            SiteId = request.SiteId,
            ClientId = clientId,
            RateAmount = request.RateAmount,
            EffectiveFrom = request.EffectiveFrom.Date,
            EffectiveTo = request.EffectiveTo?.Date,
            IsActive = true,
            CreatedBy = _currentUserService.UserId.Value,
            EpfPercent = request.EpfPercent,
            EsicPercent = request.EsicPercent,
            AllowancePercent = request.AllowancePercent,
            EpfWageCap = request.EpfWageCap
        };

        await repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<Guid>.SuccessResponse(entity.Id, "Rate plan saved");
    }
}

