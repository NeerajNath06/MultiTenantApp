using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Shifts.Commands.CreateShift;

public class CreateShiftCommandHandler : IRequestHandler<CreateShiftCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateShiftCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateShiftCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        var shift = new Shift
        {
            TenantId = _tenantContext.TenantId.Value,
            ShiftName = request.ShiftName,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            BreakDuration = request.BreakDuration,
            IsActive = true
        };

        await _unitOfWork.Repository<Shift>().AddAsync(shift, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(shift.Id, "Shift created successfully");
    }
}
