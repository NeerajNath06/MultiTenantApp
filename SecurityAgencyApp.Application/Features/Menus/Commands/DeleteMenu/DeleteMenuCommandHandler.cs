using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Menus.Commands.DeleteMenu;

public class DeleteMenuCommandHandler : IRequestHandler<DeleteMenuCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteMenuCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteMenuCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        var menuRepo = _unitOfWork.Repository<Menu>();
        var menu = await menuRepo.GetByIdAsync(request.Id, cancellationToken);

        if (menu == null || menu.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("Menu not found");
        }

        // Check if menu has submenus
        var subMenus = await _unitOfWork.Repository<SubMenu>().FindAsync(
            sm => sm.MenuId == menu.Id, cancellationToken);

        if (subMenus.Any())
        {
            return ApiResponse<bool>.ErrorResponse("Cannot delete menu with existing submenus. Please delete submenus first.");
        }

        // Soft delete - mark as inactive
        menu.IsActive = false;
        menu.ModifiedDate = DateTime.UtcNow;
        await menuRepo.UpdateAsync(menu, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Menu deleted successfully");
    }
}
