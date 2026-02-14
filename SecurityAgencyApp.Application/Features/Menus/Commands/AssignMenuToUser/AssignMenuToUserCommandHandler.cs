using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Menus.Commands.AssignMenuToUser;

public class AssignMenuToUserCommandHandler : IRequestHandler<AssignMenuToUserCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public AssignMenuToUserCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(AssignMenuToUserCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        // Verify user exists and belongs to tenant
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId, cancellationToken);
        if (user == null || user.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("User not found");
        }

        // Remove existing menu assignments
        var existingMenus = await _unitOfWork.Repository<UserMenu>().FindAsync(
            um => um.UserId == request.UserId, cancellationToken);

        foreach (var existingMenu in existingMenus)
        {
            await _unitOfWork.Repository<UserMenu>().DeleteAsync(existingMenu, cancellationToken);
        }

        // Add new menu assignments
        foreach (var menuId in request.MenuIds)
        {
            // Verify menu exists and belongs to tenant
            var menu = await _unitOfWork.Repository<Menu>().GetByIdAsync(menuId, cancellationToken);
            if (menu != null && menu.TenantId == _tenantContext.TenantId.Value)
            {
                var userMenu = new UserMenu
                {
                    UserId = request.UserId,
                    MenuId = menuId
                };
                await _unitOfWork.Repository<UserMenu>().AddAsync(userMenu, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Menus assigned to user successfully");
    }
}
