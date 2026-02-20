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

        // Resolve User.Id for ReportedBy / ReportedByUserId (FK to Users). JWT may contain Guard.Id when guard logs in from mobile.
        var reportedByUserId = _currentUserService.UserId.Value;
        var userExists = await _unitOfWork.Repository<User>().GetByIdAsync(reportedByUserId, cancellationToken);
        if (userExists == null)
        {
            var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(reportedByUserId, cancellationToken);
            if (guard != null && guard.TenantId == _tenantContext.TenantId.Value && guard.UserId.HasValue)
            {
                reportedByUserId = guard.UserId.Value;
            }
            else
            {
                return ApiResponse<Guid>.ErrorResponse("Your guard account must be linked to a user to report incidents. Please contact your administrator.");
            }
        }

        // Verify site belongs to tenant
        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(request.SiteId, cancellationToken);
        if (site == null || site.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<Guid>.ErrorResponse("Site not found");
        }

        // When GuardId is provided, ensure guard exists and belongs to tenant (avoids FK violation)
        if (request.GuardId.HasValue)
        {
            var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(request.GuardId.Value, cancellationToken);
            if (guard == null || guard.TenantId != _tenantContext.TenantId.Value)
            {
                return ApiResponse<Guid>.ErrorResponse("Selected guard not found or does not belong to this tenant.");
            }
        }

        // Load user so EF can set ReportedByUserId FK from navigation (reuse if already loaded)
        var reportedByUser = userExists ?? await _unitOfWork.Repository<User>().GetByIdAsync(reportedByUserId, cancellationToken);
        if (reportedByUser == null)
        {
            return ApiResponse<Guid>.ErrorResponse("Reporting user not found.");
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
            ReportedBy = reportedByUserId,
            ReportedByUser = reportedByUser,
            IncidentDate = request.IncidentDate,
            IncidentType = request.IncidentType,
            Severity = request.Severity,
            Description = request.Description,
            ActionTaken = request.ActionTaken,
            Status = Domain.Enums.IncidentStatus.Open
        };

        await _unitOfWork.Repository<IncidentReport>().AddAsync(incident, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return ApiResponse<Guid>.ErrorResponse($"Could not save incident: {message}");
        }

        return ApiResponse<Guid>.SuccessResponse(incident.Id, "Incident report created successfully");
    }
}
