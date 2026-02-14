using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Visitors.Queries.GetVisitorById;

public class GetVisitorByIdQueryHandler : IRequestHandler<GetVisitorByIdQuery, ApiResponse<VisitorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetVisitorByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<VisitorDto>> Handle(GetVisitorByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<VisitorDto>.ErrorResponse("Tenant context not found");

        var entity = await _unitOfWork.Repository<Visitor>().GetByIdAsync(request.Id, cancellationToken);
        if (entity == null || entity.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<VisitorDto>.ErrorResponse("Visitor not found");

        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(entity.SiteId, cancellationToken);
        var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(entity.GuardId, cancellationToken);

        var dto = new VisitorDto
        {
            Id = entity.Id,
            VisitorName = entity.VisitorName,
            VisitorType = entity.VisitorType,
            CompanyName = entity.CompanyName,
            PhoneNumber = entity.PhoneNumber,
            Email = entity.Email,
            Purpose = entity.Purpose,
            HostName = entity.HostName,
            HostDepartment = entity.HostDepartment,
            SiteId = entity.SiteId,
            SiteName = site?.SiteName,
            GuardId = entity.GuardId,
            GuardName = guard != null ? $"{guard.FirstName} {guard.LastName}".Trim() : null,
            EntryTime = entity.EntryTime,
            ExitTime = entity.ExitTime,
            IdProofType = entity.IdProofType,
            IdProofNumber = entity.IdProofNumber,
            BadgeNumber = entity.BadgeNumber,
        };
        return ApiResponse<VisitorDto>.SuccessResponse(dto, "Visitor retrieved");
    }
}
