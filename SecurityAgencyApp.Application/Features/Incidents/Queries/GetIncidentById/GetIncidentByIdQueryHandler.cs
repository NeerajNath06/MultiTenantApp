using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Incidents.Queries.GetIncidentById;

public class GetIncidentByIdQueryHandler : IRequestHandler<GetIncidentByIdQuery, ApiResponse<IncidentDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetIncidentByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<IncidentDetailDto>> Handle(GetIncidentByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<IncidentDetailDto>.ErrorResponse("Tenant context not found");

        var incident = await _unitOfWork.Repository<IncidentReport>().GetByIdAsync(request.Id, cancellationToken);
        if (incident == null || incident.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<IncidentDetailDto>.ErrorResponse("Incident not found");

        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(incident.SiteId, cancellationToken);
        string? guardName = null;
        if (incident.GuardId.HasValue)
        {
            var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(incident.GuardId.Value, cancellationToken);
            guardName = guard != null ? $"{guard.FirstName} {guard.LastName}".Trim() : null;
        }

        var dto = new IncidentDetailDto
        {
            Id = incident.Id,
            IncidentNumber = incident.IncidentNumber,
            SiteId = incident.SiteId,
            SiteName = site?.SiteName ?? "",
            GuardId = incident.GuardId,
            GuardName = guardName,
            IncidentDate = incident.IncidentDate,
            IncidentType = incident.IncidentType,
            Severity = incident.Severity.ToString(),
            Status = incident.Status.ToString(),
            Description = incident.Description,
            ActionTaken = incident.ActionTaken
        };
        return ApiResponse<IncidentDetailDto>.SuccessResponse(dto);
    }
}
