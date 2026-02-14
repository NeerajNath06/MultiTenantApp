using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.GuardAssignments.Commands.CreateAssignment;

public class CreateAssignmentCommandHandler : IRequestHandler<CreateAssignmentCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateAssignmentCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        if (!_currentUserService.UserId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("User context not found");
        }

        // Verify guard, site, and shift belong to tenant
        var guard = await _unitOfWork.Repository<SecurityGuard>().GetByIdAsync(request.GuardId, cancellationToken);
        if (guard == null || guard.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<Guid>.ErrorResponse("Guard not found");
        }

        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(request.SiteId, cancellationToken);
        if (site == null || site.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<Guid>.ErrorResponse("Site not found");
        }

        var shift = await _unitOfWork.Repository<Shift>().GetByIdAsync(request.ShiftId, cancellationToken);
        if (shift == null || shift.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<Guid>.ErrorResponse("Shift not found");
        }

        var assignment = new GuardAssignment
        {
            TenantId = _tenantContext.TenantId.Value,
            GuardId = request.GuardId,
            SiteId = request.SiteId,
            ShiftId = request.ShiftId,
            SupervisorId = request.SupervisorId,
            AssignmentStartDate = request.AssignmentStartDate,
            AssignmentEndDate = request.AssignmentEndDate,
            Status = Domain.Enums.AssignmentStatus.Active,
            Remarks = request.Remarks,
            CreatedBy = _currentUserService.UserId.Value
        };

        await _unitOfWork.Repository<GuardAssignment>().AddAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(assignment.Id, "Guard assigned successfully");
    }
}
