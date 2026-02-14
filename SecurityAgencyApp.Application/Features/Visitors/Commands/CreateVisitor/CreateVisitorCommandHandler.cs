using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Visitors.Commands.CreateVisitor;

public class CreateVisitorCommandHandler : IRequestHandler<CreateVisitorCommand, ApiResponse<CreateVisitorResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateVisitorCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<CreateVisitorResultDto>> Handle(CreateVisitorCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<CreateVisitorResultDto>.ErrorResponse("Tenant context not found");

        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(request.SiteId, cancellationToken);
        if (site == null || site.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<CreateVisitorResultDto>.ErrorResponse("Invalid site");

        var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(request.GuardId, cancellationToken);
        if (guard == null || guard.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<CreateVisitorResultDto>.ErrorResponse("Invalid guard");

        var badgeNumber = "V-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();

        var entity = new Visitor
        {
            TenantId = _tenantContext.TenantId.Value,
            VisitorName = request.VisitorName.Trim(),
            VisitorType = string.IsNullOrWhiteSpace(request.VisitorType) ? "Individual" : request.VisitorType.Trim(),
            CompanyName = request.CompanyName?.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Email = request.Email?.Trim(),
            Purpose = request.Purpose.Trim(),
            HostName = request.HostName?.Trim(),
            HostDepartment = request.HostDepartment?.Trim(),
            SiteId = request.SiteId,
            GuardId = request.GuardId,
            EntryTime = DateTime.UtcNow,
            IdProofType = request.IdProofType?.Trim(),
            IdProofNumber = request.IdProofNumber?.Trim(),
            BadgeNumber = badgeNumber,
            IsActive = true
        };

        await _unitOfWork.Repository<Visitor>().AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var result = new CreateVisitorResultDto { Id = entity.Id, BadgeNumber = entity.BadgeNumber ?? badgeNumber };
        return ApiResponse<CreateVisitorResultDto>.SuccessResponse(result, "Visitor registered");
    }
}
