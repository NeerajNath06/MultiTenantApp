using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.SubMenus.Commands.DeleteSubMenu;

public class DeleteSubMenuCommandHandler : IRequestHandler<DeleteSubMenuCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteSubMenuCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteSubMenuCommand request, CancellationToken cancellationToken)
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

        // Verify menu belongs to tenant
        var menu = await _unitOfWork.Repository<Menu>().GetByIdAsync(subMenu.MenuId, cancellationToken);
        if (menu == null || menu.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("SubMenu not found or access denied");
        }

        // Soft delete - mark as inactive
        subMenu.IsActive = false;
        subMenu.ModifiedDate = DateTime.UtcNow;
        await subMenuRepo.UpdateAsync(subMenu, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "SubMenu deleted successfully");
    }
}
