using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Shifts.Commands.UpdateShift;

public class UpdateShiftCommandHandler : IRequestHandler<UpdateShiftCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateShiftCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateShiftCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        var shiftRepo = _unitOfWork.Repository<Shift>();
        var shift = await shiftRepo.GetByIdAsync(request.Id, cancellationToken);

        if (shift == null || shift.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("Shift not found");
        }

        if (request.EndTime <= request.StartTime)
        {
            return ApiResponse<bool>.ErrorResponse("End time must be after start time");
        }

        shift.ShiftName = request.ShiftName;
        shift.StartTime = request.StartTime;
        shift.EndTime = request.EndTime;
        shift.BreakDuration = request.BreakDuration;
        shift.IsActive = request.IsActive;
        shift.ModifiedDate = DateTime.UtcNow;

        await shiftRepo.UpdateAsync(shift, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Shift updated successfully");
    }
}
