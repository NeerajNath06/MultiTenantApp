using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Incidents.Commands.CreateIncident;

public class CreateIncidentCommandHandler : IRequestHandler<CreateIncidentCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateIncidentCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateIncidentCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        if (!_currentUserService.UserId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("User context not found");
        }

        // Verify site belongs to tenant
        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(request.SiteId, cancellationToken);
        if (site == null || site.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<Guid>.ErrorResponse("Site not found");
        }

        // Generate incident number
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month.ToString("D2");
        var existingCount = await _unitOfWork.Repository<IncidentReport>().CountAsync(
            i => i.TenantId == _tenantContext.TenantId.Value && 
                 i.IncidentDate.Year == year && 
                 i.IncidentDate.Month == DateTime.UtcNow.Month,
            cancellationToken);
        
        var incidentNumber = $"INC{year}{month}{(existingCount + 1):D4}";

        var incident = new IncidentReport
        {
            TenantId = _tenantContext.TenantId.Value,
            IncidentNumber = incidentNumber,
            SiteId = request.SiteId,
            GuardId = request.GuardId,
            ReportedBy = _currentUserService.UserId.Value,
            IncidentDate = request.IncidentDate,
            IncidentType = request.IncidentType,
            Severity = request.Severity,
            Description = request.Description,
            ActionTaken = request.ActionTaken,
            Status = Domain.Enums.IncidentStatus.Open
        };

        await _unitOfWork.Repository<IncidentReport>().AddAsync(incident, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(incident.Id, "Incident report created successfully");
    }
}
