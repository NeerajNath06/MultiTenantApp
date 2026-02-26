using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.SubMenus.Commands.UpdateSubMenu;

public class UpdateSubMenuCommandHandler : IRequestHandler<UpdateSubMenuCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateSubMenuCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateSubMenuCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        var subMenuRepo = _unitOfWork.Repository<SubMenu>();
        var subMenu = await subMenuRepo.GetByIdAsync(request.Id, cancellationToken);

        if (subMenu == null)
        {
            return ApiResponse<bool>.ErrorResponse("SubMenu not found");
        }

        // Update parent menu only when MenuId is provided (non-empty)
        if (request.MenuId != Guid.Empty)
        {
            var menu = await _unitOfWork.Repository<Menu>().GetByIdAsync(request.MenuId, cancellationToken);
            if (menu == null || menu.TenantId != _tenantContext.TenantId.Value)
            {
                return ApiResponse<bool>.ErrorResponse("Menu not found or access denied");
            }
            subMenu.MenuId = request.MenuId;
        }
        subMenu.Name = request.Name;
        subMenu.DisplayName = request.DisplayName ?? request.Name;
        subMenu.Icon = request.Icon;
        subMenu.Route = request.Route;
        subMenu.DisplayOrder = request.DisplayOrder;
        subMenu.IsActive = request.IsActive;
        subMenu.ModifiedDate = DateTime.UtcNow;

        await subMenuRepo.UpdateAsync(subMenu, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "SubMenu updated successfully");
    }
}
