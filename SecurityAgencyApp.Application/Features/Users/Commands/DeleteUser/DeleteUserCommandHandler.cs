using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteUserCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.GetByIdAsync(request.Id, cancellationToken);

        if (user == null || user.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("User not found");
        }

        var userRoleRepo = _unitOfWork.Repository<UserRole>();
        var userRoles = await userRoleRepo.FindAsync(ur => ur.UserId == user.Id, cancellationToken);
        foreach (var ur in userRoles)
        {
            await userRoleRepo.DeleteAsync(ur, cancellationToken);
        }

        var userMenuRepo = _unitOfWork.Repository<UserMenu>();
        var userMenus = await userMenuRepo.FindAsync(um => um.UserId == user.Id, cancellationToken);
        foreach (var um in userMenus)
        {
            await userMenuRepo.DeleteAsync(um, cancellationToken);
        }

        await userRepo.DeleteAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "User deleted successfully");
    }
}
