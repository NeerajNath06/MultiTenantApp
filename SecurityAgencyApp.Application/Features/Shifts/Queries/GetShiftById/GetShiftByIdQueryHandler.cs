using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Shifts.Queries.GetShiftById;

public class GetShiftByIdQueryHandler : IRequestHandler<GetShiftByIdQuery, ApiResponse<ShiftDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetShiftByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<ShiftDto>> Handle(GetShiftByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<ShiftDto>.ErrorResponse("Tenant context not found");
        }

        var shiftRepo = _unitOfWork.Repository<Shift>();
        var shift = await shiftRepo.GetByIdAsync(request.Id, cancellationToken);

        if (shift == null || shift.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<ShiftDto>.ErrorResponse("Shift not found");
        }

        var shiftDto = new ShiftDto
        {
            Id = shift.Id,
            ShiftName = shift.ShiftName,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            BreakDuration = shift.BreakDuration,
            IsActive = shift.IsActive,
            CreatedDate = shift.CreatedDate,
            ModifiedDate = shift.ModifiedDate
        };

        return ApiResponse<ShiftDto>.SuccessResponse(shiftDto, "Shift retrieved successfully");
    }
}
