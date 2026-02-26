using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.SecurityGuards.Queries.GetGuardById;

public class GetGuardByIdQueryHandler : IRequestHandler<GetGuardByIdQuery, ApiResponse<GuardDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetGuardByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<GuardDto>> Handle(GetGuardByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<GuardDto>.ErrorResponse("Tenant context not found");
        }

        var guardRepo = _unitOfWork.Repository<SecurityGuard>();
        var guard = await guardRepo.GetByIdAsync(request.Id, cancellationToken);

        if (guard == null || guard.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<GuardDto>.ErrorResponse("Security guard not found");
        }

        User? supervisor = null;
        if (guard.SupervisorId.HasValue)
            supervisor = await _unitOfWork.Repository<User>().GetByIdAsync(guard.SupervisorId.Value, cancellationToken);

        var guardDto = new GuardDto
        {
            Id = guard.Id,
            GuardCode = guard.GuardCode,
            FirstName = guard.FirstName,
            LastName = guard.LastName,
            Email = guard.Email,
            PhoneNumber = guard.PhoneNumber,
            Gender = guard.Gender.ToString(),
            DateOfBirth = guard.DateOfBirth,
            Address = guard.Address,
            City = guard.City,
            State = guard.State,
            PinCode = guard.PinCode,
            AadharNumber = guard.AadharNumber,
            PANNumber = guard.PANNumber,
            EmergencyContactName = guard.EmergencyContactName,
            EmergencyContactPhone = guard.EmergencyContactPhone,
            JoiningDate = guard.JoiningDate,
            IsActive = guard.IsActive,
            CreatedDate = guard.CreatedDate,
            ModifiedDate = guard.ModifiedDate,
            SupervisorId = guard.SupervisorId,
            SupervisorName = supervisor != null ? $"{supervisor.FirstName} {supervisor.LastName}".Trim() : null,
            PhotoPath = guard.PhotoPath
        };

        return ApiResponse<GuardDto>.SuccessResponse(guardDto, "Security guard retrieved successfully");
    }
}
