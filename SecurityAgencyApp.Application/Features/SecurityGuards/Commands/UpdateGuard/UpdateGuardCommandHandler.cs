using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.SecurityGuards.Commands.UpdateGuard;

public class UpdateGuardCommandHandler : IRequestHandler<UpdateGuardCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateGuardCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateGuardCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        var guardRepo = _unitOfWork.Repository<SecurityGuard>();
        var guard = await guardRepo.GetByIdAsync(request.Id, cancellationToken);

        if (guard == null || guard.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("Security guard not found");
        }

        // Check if guard code is already taken by another guard
        if (guard.GuardCode != request.GuardCode)
        {
            var existing = await guardRepo.FirstOrDefaultAsync(
                g => g.GuardCode == request.GuardCode && g.Id != request.Id && g.TenantId == _tenantContext.TenantId.Value,
                cancellationToken);

            if (existing != null)
            {
                return ApiResponse<bool>.ErrorResponse("Guard code already exists");
            }
        }

        guard.GuardCode = request.GuardCode;
        guard.FirstName = request.FirstName;
        guard.LastName = request.LastName;
        guard.Email = request.Email;
        guard.PhoneNumber = request.PhoneNumber;
        guard.Gender = request.Gender;
        guard.DateOfBirth = request.DateOfBirth;
        guard.Address = request.Address;
        guard.City = request.City;
        guard.State = request.State;
        guard.PinCode = request.PinCode;
        guard.AadharNumber = request.AadharNumber;
        guard.PANNumber = request.PANNumber;
        guard.EmergencyContactName = request.EmergencyContactName ?? string.Empty;
        guard.EmergencyContactPhone = request.EmergencyContactPhone ?? string.Empty;
        if (request.JoiningDate.HasValue)
            guard.JoiningDate = request.JoiningDate.Value;
        guard.IsActive = request.IsActive;
        guard.SupervisorId = request.SupervisorId;
        if (request.PhotoPath != null)
            guard.PhotoPath = request.PhotoPath;
        guard.ModifiedDate = DateTime.UtcNow;

        await guardRepo.UpdateAsync(guard, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Security guard updated successfully");
    }
}
